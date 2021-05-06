using SabreFareRulesInterface.MDFSabreAirTicketService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Messaging;
using System.Threading;

namespace FareCollector
{
    internal static class ThreadManager
    {
        private static int MAX_THREAD_COUNT;
        private static int MAX_TIMOUT_ERROR;
        private static volatile int _timeoutCount = 0;

        private static string _WebServiceUrl = string.Empty;
        private static string _WebSerivceID = string.Empty;
        private static string _WebServicePassword = string.Empty;
        private static Mutex workingListMutex = new Mutex();
        private static Mutex timeoutCounterMutex = new Mutex();

        internal static int currentTimoutErrorCount
        {
            get
            {

                return _timeoutCount;
            }
            set
            {
                timeoutCounterMutex.WaitOne();
                _timeoutCount = value;
                General._ApplicationLogger.Debug("Current timeouts=" + currentTimoutErrorCount.ToString(), System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                timeoutCounterMutex.ReleaseMutex();
            }
        }

        internal static volatile EventWaitHandle errorThreadHold;
        public static volatile bool processing = false;
        public static volatile bool paused = false;
        private static volatile Queue<Thread> threadQueue = new Queue<Thread>();
        private static volatile List<Thread> processingThreadList = new List<Thread>();

        private static MSMQCommunication.Manager manager = new MSMQCommunication.Manager(AppSettings.InboundQueue, AppSettings.HP_InboundQueue, Utility.AppCodes.FARE_COLLECTOR);

        public static bool IsFinishedProcessing()
        {
            return processingThreadList.Count == 0 && !processing;
        }

        static ThreadManager()
        {
            //Reset Errors and Failed message count on this new run "Session"
            currentTimoutErrorCount = 0;

            errorThreadHold = new EventWaitHandle(false, EventResetMode.AutoReset);
            ReadAndValidateConfigurationSettings();
        }

        public static void InitializeFareProcessing()
        {
            try
            {
                paused = false;
                GenerateThreads();
                currentTimoutErrorCount = 0;
                General._ApplicationLogger.Debug("Internal timeout count reset", "Timeout");
                processing = true;
                Thread thread = new Thread(StartThreads)
                {
                    Name = "ThreadManager Main Control Thread"
                };
                General._ApplicationLogger.Debug("Main Thread Started", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                thread.Start();
            }
            catch (Exception ex)
            {
                General._ApplicationLogger.Error("Error starting Main control thread " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
            }
        }

        private static void GenerateThreads()
        {
            threadQueue.Clear();
            for (int i = 0; i < MAX_THREAD_COUNT; i++)
            {
                Thread temp = new Thread(tf_ProcessFare);
                temp.Name = "Worker Thread " + i.ToString() + " ID:" + temp.ManagedThreadId.ToString();
                threadQueue.Enqueue(temp);
                General._ApplicationLogger.Info(temp.Name + " created Successfully");
            }
        }

        private static void StartThreads()
        {
            try
            {
                do
                {
                    //Check if currently operating threads have exceeded the timeout error limit
                    if (currentTimoutErrorCount >= MAX_TIMOUT_ERROR)
                    {
                        paused = true;
                        General._ApplicationLogger.Warn("Worker threads paused due to multiple conneciton errors.", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                        new Thread(tf_Timeout).Start();
                        errorThreadHold.WaitOne();
                        paused = false;
                        currentTimoutErrorCount = 0;
                        GenerateThreads();
                        General._ApplicationLogger.Debug("Internal timeout count reset", "Timeout Check");
                    }
                    //Only release new threads if they are available and the queue has a msg
                    else if (threadQueue.Count > 0
                        && (manager.NextMessageAvailable(MessagePriority.Normal) || manager.NextMessageAvailable(MessagePriority.High))
                        && currentTimoutErrorCount < MAX_TIMOUT_ERROR)
                    {
                        Thread temp = threadQueue.Dequeue();
                        try
                        {
                            if (temp != null)
                            {
                                temp.Start();
                                processingThreadList.Add(temp);
                            }
                        }
                        catch (Exception ex)
                        {
                            General._ApplicationLogger.Error("Error starting thread " + temp.Name + ex.Message + " Application shutting down");
                        }
                    }
                    //Processing has been completed when the threads have completed and there are no more MSMQ messages
                    else if (!manager.NextMessageAvailable(MessagePriority.Normal) && !manager.NextMessageAvailable(MessagePriority.High))
                    {
                        processing = false;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
                while (processing);

                //Wait for currrently operating threads to shut down. (This loop should in theory never be used)
                if (processingThreadList.Count > 0)
                {
                    General._ApplicationLogger.Info("Waiting for " + processingThreadList.Count + " to complete");
                    WaitForThreadCompleteion();
                }
            }
            catch (Exception ex)
            {
                General._ApplicationLogger.Error("Error starting thread Jobs :" + ex.Message + ex.InnerException.Message);
            }
            //Start the listener and end all threads
            FinishedProcessing();
        }

        private static void WaitForThreadCompleteion()
        {
            //Wait for all threads to join
            for (int i = 0; i <= 10 && processingThreadList.Count > 0; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        private static void tf_ProcessFare()
        {
            SabreAirTicketServicePortChannel channel = CreateChannel();
            do
            {
                try
                {
                    WorkerProcess worker = new WorkerProcess(channel);
                    worker.Start();
                }
                catch (Exception ex)
                {
                    General._ApplicationLogger.Error("Unexpected error in collector " + Thread.CurrentThread.Name + " " + ex.Message);
                }
            } while (processing && !paused);

            if (channel != null)
            {
                channel.Close();
                channel.Dispose();
            }

            EndThreadProcessing();
        }

        private static SabreAirTicketServicePortChannel CreateChannel()
        {
            try
            {
                SabreFareRulesInterface.SabreWebServiceFactory factory = new SabreFareRulesInterface.SabreWebServiceFactory();
                return factory.CreateChannel(_WebSerivceID, _WebServicePassword, _WebServiceUrl);
            }
            catch (Exception ex)
            {
                ThreadManager.currentTimoutErrorCount++;
                General._ApplicationLogger.Error(Thread.CurrentThread.Name + " - Failed to create channel with CSC - " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw new Exceptions.UnableToCreateChannelException("Failed to create channel with CSC - " + ex.Message, ex.InnerException);
            }
        }

        private static void EndThreadProcessing()
        {
            workingListMutex.WaitOne();
            processingThreadList.Remove(Thread.CurrentThread);
            workingListMutex.ReleaseMutex();
        }

        private static void ReadAndValidateConfigurationSettings()
        {
            MAX_THREAD_COUNT = Properties.Settings.Default.MaxThreads;
            if (MAX_THREAD_COUNT < 1)
            {
                General._ApplicationLogger.Error("Cannot start applicaiton THREADCOUNT in configuration is set to <1. Application cannot continue.", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw new ApplicationException("THREADCOUNT in configuration is set to <1. Application cannot continue");
            }
            MAX_TIMOUT_ERROR = Properties.Settings.Default.MaxTimeoutErrors;
            if (MAX_TIMOUT_ERROR < 1)
            {
                General._ApplicationLogger.Error("Cannot start applicaiton MAX_TIMOUT_ERROR in configuration is set to <1. Application cannot continue.", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw new ApplicationException("MAX_TIMOUT_ERROR in configuration is set to <1. Application cannot continue");
            }
            _WebServiceUrl = ConfigurationManager.AppSettings["SabreURL"];
            _WebSerivceID = ConfigurationManager.AppSettings["SabreID"];
            _WebServicePassword = ConfigurationManager.AppSettings["SabreBadWord"];
        }

        private static void tf_Timeout()
        {
            Thread.Sleep(TimeSpan.FromMinutes(AppSettings.TimeoutSleepDuration));
            int waitLoop = 0;
            while (processingThreadList.Count > 0 & waitLoop <= 6)
            {
                waitLoop++;
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            errorThreadHold.Set();
        }

        private static void FinishedProcessing()
        {
            threadQueue.Clear();
            QueueListener.WakeEvent.Set();
        }

        internal static void StopProcessing()
        {
            processing = false;
            errorThreadHold.Set();
        }
    }
}