using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace FareCollector
{
    internal class MainProcessor
    {
        private QueueListener listener;
        private Thread listenerThread;
        internal bool threadsJoined
        {
            get { return listenerJoined && ThreadManager.IsFinishedProcessing(); }
        }
        private bool listenerJoined;
        public MainProcessor()
        {

        }

        internal void Start()
        {
            Utility.General.EncryptDatabaseConnectionString();
            System.Net.ServicePointManager.DefaultConnectionLimit = AppSettings.MaxSabreSessions;
            while (!MSMQRunning())
            {
                General._ActivityLogger.WriteLogEntry("Could not detect MSMQ service. Waiting 30 seconds...");
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
            General._ActivityLogger.WriteLogEntry(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " " + System.Reflection.MethodInfo.GetCurrentMethod().Name);
            string startmsg = Utility.General.BuildStandardProcessStartStopMessage("Fare Collector", "Fare Collector has STARTED", AppSettings.ClientBase);
            Utility.General.SendEmailMessage(startmsg, startmsg, AppSettings.DocBankSupportEmail);
            try
            {
                listenerThread = new Thread(tl_StartListener);
                listenerThread.Name = "Listener Main Thread";
                listenerThread.Start();
                listenerJoined = false;
            }
            catch (Exception ex)
            {
                string errorMsg = "Error attempting to start listener Process. Exception Message = " + ex.Message + " " + ex.TargetSite.ReflectedType.Name + ":" + ex.TargetSite.Name;
                General._ApplicationLogger.Error(errorMsg, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                throw new ApplicationException(errorMsg);
            }
        }

        private bool MSMQRunning()
        {
            List<ServiceController> services = ServiceController.GetServices().ToList();
            ServiceController msQue = services.Find(o => o.ServiceName == "MSMQ");
            if (msQue != null)
            {
                if (msQue.Status == ServiceControllerStatus.Running)
                {
                    General._ApplicationLogger.Debug("MSMQ Service Detected", System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                    return true;
                }
            }
            else
            {
                return false;
            }
            return false;
        }
        private void tl_StartListener(object input)
        {
            listener = new QueueListener();
            listener.Start();
            listenerJoined = true;
        }

        internal void Stop()
        {
            General._ActivityLogger.WriteLogEntry(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " " + System.Reflection.MethodInfo.GetCurrentMethod().Name);
                       
            listener.Shutdown = true;
            ThreadManager.StopProcessing();
        }
    }
}