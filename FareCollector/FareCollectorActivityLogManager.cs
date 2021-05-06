using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FareCollector
{
    class FareCollectorActivityLogManager : LoggingManager.ActivityLogManager
    {
        public FareCollectorActivityLogManager(Utility.Environment appEnv, Utility.ClientBase clientBse)
            : base(LoggingManager.General.DocBankApplication.FARE_COLLECTOR, appEnv, clientBse)
        {
            // place holder
        }


        public void WriteLogEntry(string activityEntry, DateTime startTime, DateTime endTime, int count, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");
            formatedLogEntry.Append(activityEntry + " ");

            WriteLogEntry(formatedLogEntry.ToString(), startTime, endTime, count);
        }

        public void WriteLogEntry(string activityEntry, DateTime startTime, DateTime endTime, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");
            formatedLogEntry.Append(activityEntry + " ");

            WriteLogEntry(formatedLogEntry.ToString(), startTime, endTime);
        }
    }
}
