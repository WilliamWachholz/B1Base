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
                AddLog("Starting");                

                timer = new System.Timers.Timer(Convert.ToInt32(ConfigurationSettings.AppSettings.Get("TimeInMiliseconds")));
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
                timer.Enabled = true;
                timer.Start();
                
                AddLog("Started");
            }
            catch (Exception exception)
            {
                AddLog("Starting error: " + exception.Message);
            }
        }

        protected override void OnStop()
        {
            AddLog("Stoped");
        }

        protected void AddLog(string msg)
        {
            EventLog.WriteEntry(ServiceName, msg, System.Diagnostics.EventLogEntryType.Information);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                AddLog("Initializing");

                AddLog("Executing");

                B1Base.Controller.ConnectionController.Instance.Initialize(ConfigurationSettings.AppSettings.Get("AddOnId"),
                    ConfigurationSettings.AppSettings.Get("Server"),
                    ConfigurationSettings.AppSettings.Get("CompanyDB"),
                    ConfigurationSettings.AppSettings.Get("UserName"),
                    ConfigurationSettings.AppSettings.Get("Password"),
                    ConfigurationSettings.AppSettings.Get("LicenseServer"),
                    ConfigurationSettings.AppSettings.Get("DBUserName"),
                    ConfigurationSettings.AppSettings.Get("DBPassword"),
                    ConfigurationSettings.AppSettings.Get("DBServerType"));

                Execute();

                AddLog("Executed");

                B1Base.Controller.ConnectionController.Instance.Finalize();

                AddLog("Finalized");
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);
            }
        }







        

    }
}
