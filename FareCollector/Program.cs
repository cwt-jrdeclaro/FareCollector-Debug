using System;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace FareCollector
{
    internal static class Program
    {
        private static DocumentBankFareCollector srv;
        private static MainProcessor main;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] Args)
        {
            AppSettings.ReadConfigurationSettings();
            if (Args.Length > 0)
            {
                if (Args[0].ToUpper() == "FORM")
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new DebugStartupForm());
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new DocumentBankFareCollector()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }

        public static void Start()
        {
            srv = new DocumentBankFareCollector();
            AppSettings.ReadConfigurationSettings();
            General._ApplicationLogger.Info("FARE COLLECTOR STARTED - " + DateTime.Now.ToString());
            
            main = new MainProcessor();
            main.Start();
        }

        public static void Stop()
        {
            main.Stop();
            while (main.threadsJoined == false)
            {
                Thread.Sleep(500);
            }
        }
    }
}