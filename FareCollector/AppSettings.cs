using System;
using System.Configuration;

namespace FareCollector
{
    internal static class AppSettings
    {
        private static string _DBConnString = string.Empty;
        private static string _DocBankSupportEmail = string.Empty;
        private static Utility.ClientBase _ClientBase;
        private static Utility.Environment _Envrionment;
        private static string _InboundQueue;
        private static string _OutboundQueue;
        private static string _HP_InboundQueue;
        private static string _HP_OutboundQueue;
        private static int _ListenerWaitInterval;
        private static int _MaxTimeoutErrors;
        private static int _TimeoutSleepDuration;
        private static int _MaxSabreSessions;
        private static int _TimeoutSetting;

        internal static string DBConnString
        {
            get { return _DBConnString; }
        }

        internal static string DocBankSupportEmail
        {
            get
            {
                return _DocBankSupportEmail;
            }
        }

        public static Utility.Environment Enviroment
        {
            get
            {
                return _Envrionment;
            }
        }

        public static Utility.ClientBase ClientBase
        {
            get
            {
                return _ClientBase;
            }
        }

        public static string OutboundQueue
        {
            get
            {
                return _OutboundQueue;
            }
        }

        public static string InboundQueue
        {
            get
            {
                return _InboundQueue;
            }
        }

        public static int ListenerWaitInterval
        {
            get
            {
                return _ListenerWaitInterval;
            }
        }

        public static int MaxTimeoutErros
        {
            get
            {
                return _MaxTimeoutErrors;
            }
        }

        public static int TimeoutSleepDuration
        {
            get
            {
                return _TimeoutSleepDuration;
            }
        }

        public static string HP_InboundQueue
        {
            get
            {
                return _HP_InboundQueue;
            }
        }

        public static string HP_OutboundQueue
        {
            get
            {
                return _HP_OutboundQueue;
            }
        }

        public static int MaxSabreSessions
        {
            get
            {
                return _MaxSabreSessions;
            }
        }

        public static int TimeoutSetting
        {
            get
            {
                return _TimeoutSetting;
            }
        }
        public static void ReadConfigurationSettings()
        {
            Tuple<bool, string> result;

            ReadConfigurationFile();
            result = ValidateSettings();

            if (result.Item1 == false)
            {
                throw new ApplicationException("Error in validation " + result.Item2);
            }
        }

        private static void ReadConfigurationFile()
        {
            try
            {
                _DBConnString = ConfigurationManager.ConnectionStrings["OracleConnection"].ToString();
                _ClientBase = (Utility.ClientBase)Enum.Parse(typeof(Utility.ClientBase), ConfigurationManager.AppSettings["ClientBase"].ToUpper());
                _Envrionment = (Utility.Environment)Enum.Parse(typeof(Utility.Environment), ConfigurationManager.AppSettings["Enviroment"].ToUpper());
                _DocBankSupportEmail = Properties.Settings.Default.DocBankSupportEmail;
                _OutboundQueue = System.Environment.MachineName + Properties.Settings.Default.OutboundQueueName;
                _InboundQueue = System.Environment.MachineName + Properties.Settings.Default.InboundQueueName;
                _ListenerWaitInterval = Properties.Settings.Default.ListenerCheckInterval;
                _TimeoutSleepDuration = Properties.Settings.Default.TimeoutSleepDuration;
                _MaxTimeoutErrors = Properties.Settings.Default.MaxTimeoutErrors;
                _HP_OutboundQueue = System.Environment.MachineName + Properties.Settings.Default.HP_OutboundQueueName;
                _HP_InboundQueue = System.Environment.MachineName + Properties.Settings.Default.HP_InboundQueueName;
                _MaxSabreSessions = Properties.Settings.Default.MaxSabreSessions;
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("Error reading configuration file: " + ex.Message);
            }
        }

        private static Tuple<bool, string> ValidateSettings()
        {
            bool status = true;
            string errorMessage = string.Empty;

            if (status == true)
            {
                if (_DBConnString == string.Empty)
                {
                    status = false;
                    errorMessage = "Missing DBConnString variable in .config";
                }
            }
            if (status == true)
            {
                if (_ClientBase != Utility.ClientBase.COMMERCIAL && _ClientBase != Utility.ClientBase.MILTGOV)
                {
                    status = false;
                    errorMessage = "Missing Client Base variable in .config";
                }
            }
            if (status == true)
            {
                if (_Envrionment != Utility.Environment.DEVELOPMENT && _Envrionment != Utility.Environment.TEST && _Envrionment != Utility.Environment.PRODUCTION)
                {
                    status = false;
                    errorMessage = "Missing Enviroment variable in .config";
                }
            }
            if (!status)
            {
                throw new ApplicationException(errorMessage);
            }
            return new Tuple<bool, string>(status, errorMessage);
        }
    }
}