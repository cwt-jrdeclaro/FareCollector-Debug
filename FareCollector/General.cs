using log4net;
using System.IO;
using System.Xml.Serialization;

namespace FareCollector
{
    internal static class General
    {
        // these are the LOG4Net Loggers
        internal static FareCollector.FareCollectorActivityLogManager _ActivityLogger;

        internal static FareCollector.FareCollectorApplicationLogManager _ApplicationLogger;

        internal static FareCollector.FareCollectorFailedMessageLogManager _FailedMessageLogger;       

        static General()
        {
            Initialize();
        }


        private static void Initialize()
        {

            _ActivityLogger = new FareCollectorActivityLogManager(AppSettings.Enviroment, AppSettings.ClientBase);
            _ApplicationLogger = new FareCollectorApplicationLogManager(AppSettings.Enviroment, AppSettings.ClientBase);
            _FailedMessageLogger = new FareCollectorFailedMessageLogManager(AppSettings.Enviroment, AppSettings.ClientBase);            

        }

        //public static T DeserializeObject<T>(String pXmlizedString)
        //{
        //    XmlSerializer xs = new XmlSerializer(typeof(T));

        //    MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));

        //    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);

        //    return (T)xs.Deserialize(memoryStream);
        //}

        public static T DeserializeXMLToSabreObject<T>(string xml)
        {
            //xml = xml.Remove(61, 112);
            var serializer = new XmlSerializer(typeof(T), "http://webservices.sabre.com/sabreXML/2003/07");
            T result;

            using (TextReader reader = new StringReader(xml))
            {
                result = (T)serializer.Deserialize(reader);
            }

            return result;
        }
    }
}