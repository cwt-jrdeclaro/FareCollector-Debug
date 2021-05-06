using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.ServiceProcess;

namespace FareCollector
{
    [RunInstaller(true)]
    public partial class FareCollectorInstaller : System.Configuration.Install.Installer
    {
        public FareCollectorInstaller()
        {
            InitializeComponent();

            Configuration config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string baseName = GetServiceNameAppConfig("ClientBase");
            //string baseName = (string)Enum.Parse(typeof(Utility.ClientBase), ConfigurationManager.AppSettings["ClientBase"].ToUpper());

            // the section below will dynamically create the service name based on the configuration file section that was read above

            serviceInstaller1.DisplayName = "DocBank_FareCollector_" + baseName;
            serviceInstaller1.ServiceName = "DocBank_FareCollector_" + baseName;
            serviceInstaller1.Description = "Document Bank Fare Collector for " + baseName;
            serviceInstaller1.StartType = ServiceStartMode.Automatic;
        }

        private string GetServiceNameAppConfig(string serviceName)
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetAssembly(typeof(FareCollector.FareCollectorInstaller)).Location);
            return config.AppSettings.Settings[serviceName].Value;
        }
    }
}