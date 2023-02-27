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
        static System.Timers.Timer timer2;
        static bool m_ExecutingMain;
        public abstract string ServiceName { get; }
        public abstract string ServiceTitle { get; }
        public abstract string ServiceDescription { get; }

        protected virtual void Init()
        {

        }

        public virtual void Execute()
        {

        }

        protected virtual void ExtraExecute()
        {

        }

        protected virtual bool LaziInitialization { get { return false; } }

        protected override void OnStart(string[] args)
        {
            try
            {
                AddLog("Starting");

                AddLog("Initializing");

                Init();

                if (Convert.ToInt32(ConfigurationSettings.AppSettings.Get("TimeInMiliseconds")) > 0)
                {
                    timer = new System.Timers.Timer(Convert.ToInt32(ConfigurationSettings.AppSettings.Get("TimeInMiliseconds")));
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
                    timer.Enabled = true;
                    timer.Start();
                }

                if (Convert.ToInt32(ConfigurationSettings.AppSettings.Get("ExtraTimeInMiliseconds")) > 0)
                {
                    timer2 = new System.Timers.Timer(Convert.ToInt32(ConfigurationSettings.AppSettings.Get("ExtraTimeInMiliseconds")));
                    timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_Tick);
                    timer2.Enabled = true;
                    timer2.Start();
                }

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
                if (m_ExecutingMain == false)
                {
                    m_ExecutingMain = true;
                    try
                    {
                        if (!LaziInitialization)
                        {
                            AddLog("Connecting");

                            B1Base.Controller.ConnectionController.Instance.Initialize(ConfigurationSettings.AppSettings.Get("AddOnId"),
                                ConfigurationSettings.AppSettings.Get("Server"),
                                ConfigurationSettings.AppSettings.Get("CompanyDB"),
                                ConfigurationSettings.AppSettings.Get("UserName"),
                                ConfigurationSettings.AppSettings.Get("Password"),
                                ConfigurationSettings.AppSettings.Get("LicenseServer"),                                
                                ConfigurationSettings.AppSettings.Get("DBUserName"),
                                ConfigurationSettings.AppSettings.Get("DBPassword"),
                                ConfigurationSettings.AppSettings.Get("DBServerType"),
                                ConfigurationSettings.AppSettings.Get("SLDServer"));
                        }

                        AddLog("Executing");

                        Execute();

                        AddLog("Executed");
                    }
                    finally
                    {
                        m_ExecutingMain = false;
                        if (!LaziInitialization)
                        {
                            AddLog("Desconnecting");

                            B1Base.Controller.ConnectionController.Instance.Finalize();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);

                Environment.Exit(-1);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                if (m_ExecutingMain == false)
                {
                    try
                    {
                        AddLog("Extra Executing");

                        ExtraExecute();

                        AddLog("Extra Executed");
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);

                Environment.Exit(-1);
            }
        }

        public string AddOnID
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("AddOnID");
            }
        }

        public string Server
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("Server");
            }
        }

        public string CompanyDB
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("CompanyDB");
            }
        }

        public string UserName
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("UserName");
            }
        }

        public string Password
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("Password");
            }
        }

        public string LicenseServer
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("LicenseServer");
            }
        }

        public string DBUserName
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("DBUserName");
            }
        }

        public string DBPassword
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("DBPassword");
            }
        }

        public string DBServerType
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("DBServerType");
            }
        }





    }
}
