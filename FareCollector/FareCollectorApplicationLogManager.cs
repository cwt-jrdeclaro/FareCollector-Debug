using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FareCollector
{
    class FareCollectorApplicationLogManager : LoggingManager.ApplicationLogManager
    {
        public FareCollectorApplicationLogManager(Utility.Environment appEnv, Utility.ClientBase clientBse)
            : base(LoggingManager.General.DocBankApplication.FARE_COLLECTOR, appEnv, clientBse)
        {
            // place holder
        }


        public void Debug(string className, string methodName, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();

            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");

            Debug(formatedLogEntry.ToString(), className, methodName);
        }

        public void Debug(string generalMessage, string className, string methodName, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();

            formatedLogEntry.Append("GeneralMessage=\"" + generalMessage + "\" ");
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");

            Debug(formatedLogEntry.ToString(), className, methodName);
        }

        public void Error(string generalMessage, string exceptionMessage, string className, string methodName, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();
            formatedLogEntry.Append("GeneralMessage=\"" + generalMessage + "\" ");
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");

            Error(formatedLogEntry.ToString(), exceptionMessage, className, methodName);
        }


        public void Error(string generalMessage, string className, string methodName, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();
            formatedLogEntry.Append("GeneralMessage=\"" + generalMessage + "\" ");
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");

            Error(formatedLogEntry.ToString(), className, methodName);
        }

        public void Info(string generalMessage, string className, string methodName, Utility.SupplierService supplierSrvc)
        {
            StringBuilder formatedLogEntry = new StringBuilder();

            formatedLogEntry.Append("GeneralMessage=\"" + generalMessage + "\" ");
            formatedLogEntry.Append("SupplierService=\"" + supplierSrvc.ToString() + "\" ");

            Info(formatedLogEntry.ToString(), className, methodName);
        }

    }
}
