using log4net.Config;
using System;
using System.ServiceProcess;

namespace FareCollector
{
    public partial class DocumentBankFareCollector : ServiceBase
    {
        private MainProcessor main;

        public DocumentBankFareCollector()
        {
            InitializeComponent();
            XmlConfigurator.Configure();
        }

        protected override void OnStart(string[] args)
        {
            AppSettings.ReadConfigurationSettings();
            General._ActivityLogger.WriteLogEntry("Fare Collector STARTED - " + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss"));
            main = new MainProcessor();
            main.Start();
        }

        protected override void OnStop()
        {
            main.Stop();
            while (main.threadsJoined == false)
            {
                RequestAdditionalTime(25000);
            }
            string startmsg = Utility.General.BuildStandardProcessStartStopMessage("Fare Collector", "Fare Collector has STOPPED", AppSettings.ClientBase);
            Utility.General.SendEmailMessage(startmsg, startmsg, AppSettings.DocBankSupportEmail);

            base.OnStop();
            General._ActivityLogger.WriteLogEntry("FARE COLLECTOR STOPPED - " + DateTime.Now.ToString() + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + System.Reflection.MethodInfo.GetCurrentMethod().Name);
        }

        protected override void OnShutdown()
        {
            string startmsg = Utility.General.BuildStandardProcessStartStopMessage("Fare Collector", "Fare Collector has SHUTDOWN", AppSettings.ClientBase);
            Utility.General.SendEmailMessage(startmsg, startmsg, AppSettings.DocBankSupportEmail);

            main.Stop();
            while (main.threadsJoined == false)
            {
                RequestAdditionalTime(25000);
            }

            General._ActivityLogger.WriteLogEntry("Fare Collector SHUTDOWN - " + DateTime.Now.ToString() + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + System.Reflection.MethodInfo.GetCurrentMethod().Name);

            base.OnShutdown();
        }
    }
}