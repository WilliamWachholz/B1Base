using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Configuration;

namespace B1Base.Controller
{
    public abstract class ServiceController : ServiceBase
    {
        static System.Timers.Timer timer;

        public abstract string ServiceName { get; }
        public abstract string ServiceTitle { get; }
        public abstract string ServiceDescription { get; }

        protected abstract void Execute();

        protected override void OnStart(string[] args)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "Starting.", System.Diagnostics.EventLogEntryType.Information);

                B1Base.Controller.ConnectionController.Instance.Initialize(ConfigurationSettings.AppSettings.Get("AddOnId"),
                    ConfigurationSettings.AppSettings.Get("Server"),
                    ConfigurationSettings.AppSettings.Get("CompanyDB"),
                    ConfigurationSettings.AppSettings.Get("UserName"),
                    ConfigurationSettings.AppSettings.Get("Password"),
                    ConfigurationSettings.AppSettings.Get("LicenseServer"),
                    ConfigurationSettings.AppSettings.Get("DBUserName"),
                    ConfigurationSettings.AppSettings.Get("DBPassword"),
                    ConfigurationSettings.AppSettings.Get("DBServerType"));


                timer = new System.Timers.Timer(Convert.ToInt32(ConfigurationSettings.AppSettings.Get("TimeInMiliseconds")));
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
                timer.Enabled = true;
                timer.Start();

                EventLog.WriteEntry(ServiceName, "Started.", System.Diagnostics.EventLogEntryType.SuccessAudit);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(ServiceName, "Starting error: " + exception.Message, System.Diagnostics.EventLogEntryType.FailureAudit);
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry(ServiceName, "Stoping.", System.Diagnostics.EventLogEntryType.Information);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                EventLog.WriteEntry(ServiceName, "Executing.", System.Diagnostics.EventLogEntryType.Information);

                Execute();

                EventLog.WriteEntry(ServiceName, "Executed.", System.Diagnostics.EventLogEntryType.SuccessAudit);
            }
            catch (Exception exception)
            {
                EventLog.WriteEntry(ServiceName, "Execution error: " + exception.Message, System.Diagnostics.EventLogEntryType.FailureAudit);
            }
        }







        

    }
}
