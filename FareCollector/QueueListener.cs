using System;
using System.Messaging;
using System.Threading;

namespace FareCollector
{
    internal class QueueListener
    {
        private MessageQueue outputQueue;
        private MessageQueue inputQueue;
        private MessageQueue HP_outputQueue;
        private MessageQueue HP_inputQueue;
        private string _queueInputName;
        private string _queueOutputName;
        private string _HP_queueInputName;
        private string _HP_queueOutputName;
        internal bool watchQueueFlag;
        internal volatile bool Shutdown = false;
        public static volatile EventWaitHandle WakeEvent;

        public QueueListener()
        {
            WakeEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            watchQueueFlag = true;
            this._queueInputName = AppSettings.InboundQueue;
            this._queueOutputName = AppSettings.OutboundQueue;
            this._HP_queueInputName = AppSettings.HP_InboundQueue;
            this._HP_queueOutputName = AppSettings.HP_OutboundQueue;

            try
            {
                ValidateQueuesExist();
            }
            catch (Exception ex)
            {
                General._ApplicationLogger.Error("Could not find input and/or output queue. Check queue names in .config");
                throw ex;
            }

            outputQueue = new MessageQueue(_queueOutputName);
            inputQueue = new MessageQueue(_queueInputName);

            HP_outputQueue = new MessageQueue(_HP_queueOutputName);
            HP_inputQueue = new MessageQueue(_HP_queueInputName);

            outputQueue.MessageReadPropertyFilter.TransactionId = true;
            inputQueue.MessageReadPropertyFilter.TransactionId = true;
            HP_inputQueue.MessageReadPropertyFilter.TransactionId = true;
            HP_outputQueue.MessageReadPropertyFilter.TransactionId = true;
        }

        internal void Start()
        {
            General._ActivityLogger.WriteLogEntry("Listener Started at " + DateTime.Now.ToString());
            WatchQueue();
            General._ActivityLogger.WriteLogEntry("Listener Sleeping at " + DateTime.Now.ToString());
        }

        private void ValidateQueuesExist()
        {
            if (!MessageQueue.Exists(_queueInputName))
            {
                throw new ApplicationException("Listener: Input Queue " + _queueInputName + " Not Found");
            }
            if (!MessageQueue.Exists(_queueOutputName))
            {
                throw new ApplicationException("Listener: Output Queue " + _queueOutputName + "  Not Found");
            }

            if (!MessageQueue.Exists(_HP_queueInputName))
            {
                throw new ApplicationException("Listener: Input Queue " + _HP_queueInputName + " Not Found");
            }
            if (!MessageQueue.Exists(_HP_queueOutputName))
            {
                throw new ApplicationException("Listener: Output Queue " + _HP_queueOutputName + "  Not Found");
            }
        }

        private void WatchQueue()
        {
            inputQueue.MessageReadPropertyFilter.TransactionId = true;
            while (!Shutdown)
            {
                Message msg = new Message();
                try
                {
                    msg = HP_inputQueue.Peek(TimeSpan.FromSeconds(AppSettings.ListenerWaitInterval / 2));
                }
                catch (MessageQueueException ex)
                {
                    if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        if (Shutdown)
                        {
                            return;
                        }
                    }
                    else
                    {
                        General._ApplicationLogger.Info("Listener error checking queue: " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                        throw ex;
                    }
                }
                if (string.IsNullOrEmpty(msg.TransactionId))
                {
                    try
                    {
                        msg = inputQueue.Peek(TimeSpan.FromSeconds(AppSettings.ListenerWaitInterval / 2));
                    }
                    catch (MessageQueueException ex)
                    {
                        if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                        {
                            if (Shutdown)
                            {
                                return;
                            }
                        }
                        else
                        {
                            General._ApplicationLogger.Info("Listener error checking queue: " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name);
                            throw ex;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(msg.TransactionId))
                {
                    General._ActivityLogger.WriteLogEntry("Listener Detected Message(s) at " + DateTime.Now.ToString() + " on the " + _queueInputName + " queue. Starting Worker Threads");

                    OnMessageAdded();
                    //Sleep Thread Until Workers Finish Processing all Documents
                    WakeEvent.WaitOne();
                }
                if (Shutdown)
                {
                    return;
                }
            }
        }

        private void OnMessageAdded()
        {
            ThreadManager.InitializeFareProcessing();
        }
    }
}