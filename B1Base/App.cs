using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;

namespace B1Base
{
    public class App
    {
        App() { }

        static readonly App m_Instance = new App();

        public static App Instance
        {
            get { return m_Instance; }
        }

        View.AppView m_AppView = null;

        public Controller.ConnectionController ConnectionController { get { return Controller.ConnectionController.Instance; } }

        public void Initialize(View.AppView appView)
        {
            m_AppView = appView;

            m_AppView.Show();

            if (m_AppView.LaziInitialization == false)
            {
                AddTextLog("Conectando ao SAP Business One...");

                Initialize();
            }

            ClearLog();

            m_AppView.Ready();
        }
        
        public void Initialize()
        {
            if (ConfigurationSettings.AppSettings.Get("SingleSign") == "true")
            {
                B1Base.Controller.ConnectionController.Instance.Initialize(AddOnId, true);
            }
            else
            {
                B1Base.Controller.ConnectionController.Instance.Initialize(AddOnId,
                    Server,
                    CompanyDB,
                    UserName,
                    Password,
                    LicenseServer,
                    DBUserName,
                    DBPassword,
                    DBServerType);
            }
        }

        public void AddTextLog(string text)
        {
            if (m_AppView != null)
            {
                m_AppView.AddTextLog(text);
            }
        }

        public void ClearLog()
        {
            if (m_AppView != null)
            {
                m_AppView.ClearLog();
            }
        }

        public void ClearProgressBar()
        {
            if (m_AppView != null)
            {
                m_AppView.ClearProgressBar();
            }
        }

        public void SetProgressBarMax(int value) 
        { 
            if (m_AppView != null)
            {
                m_AppView.SetProgressBarMax(value);
            }
        }

        public void IncrementProgressBar(int value)
        {
            if (m_AppView != null)
            {
                m_AppView.IncrementProgressBar(value);
            }
        }

        public string CurrentDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            }
        }

        public string AddOnId
        {
            get
            {
                return ConfigurationSettings.AppSettings.Get("AddOnId");
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

        public void Close()
        {
            Application.Exit();
        }
    }
}
