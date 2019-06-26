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

            AddTextLog("Conectando ao SAP Business One...");

            Initialize();

            ClearLog();

            m_AppView.Ready();
        }
        
        public void Initialize()
        {
            B1Base.Controller.ConnectionController.Instance.Initialize(ConfigurationSettings.AppSettings.Get("AddOnId"),
                ConfigurationSettings.AppSettings.Get("Server"),
                ConfigurationSettings.AppSettings.Get("CompanyDB"),
                ConfigurationSettings.AppSettings.Get("UserName"),
                ConfigurationSettings.AppSettings.Get("Password"),
                ConfigurationSettings.AppSettings.Get("LicenseServer"),
                ConfigurationSettings.AppSettings.Get("DBUserName"),
                ConfigurationSettings.AppSettings.Get("DBPassword"),
                ConfigurationSettings.AppSettings.Get("DBServerType"));

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

        public void Close()
        {
            Application.Exit();
        }
    }
}
