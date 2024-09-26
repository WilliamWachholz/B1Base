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
using System.Reflection;
using System.IO;

namespace B1Base
{
    public class Service
    {
        static readonly Service m_Instance = new Service();

        public static Service Instance
        {
            get { return m_Instance; }
        }

        Controller.ServiceController m_ServiceController = null;

        public Controller.ConnectionController ConnectionController { get { return Controller.ConnectionController.Instance; } }

        public void Initialize(Controller.ServiceController serviceController)
        {
            m_ServiceController = serviceController;

            try
            {
                
                if (Environment.GetCommandLineArgs().Count() > 1 && Environment.GetCommandLineArgs().GetValue(1).ToString().Trim() == "DEV")
                {
                    try
                    {
                        Assembly assembly = Assembly.GetEntryAssembly();

                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.Name.Contains("Service1"))
                            {
                                var instance = Activator.CreateInstance(type);

                                ((Controller.ServiceController)instance).Execute();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    if (ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == m_ServiceController.ServiceName) == null)
                    {
                        Install();
                    }
                    else
                    {
                        bool uninstall = Environment.GetCommandLineArgs().Count() > 1 && Environment.GetCommandLineArgs().GetValue(1).ToString() == "uninstall";

                        if (uninstall)
                        {
                            Uninstall();
                        }
                        else
                        {
                            EventLog.WriteEntry(m_ServiceController.ServiceName, "Iniciado", EventLogEntryType.Information);
                            ServiceBase[] ServicesToRun;
                            ServicesToRun = new ServiceBase[] { (ServiceBase)m_ServiceController };

                            ServiceBase.Run(ServicesToRun);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry(m_ServiceController.ServiceName, "General Failure: " + ex.Message, System.Diagnostics.EventLogEntryType.Information);
            }
        }

        void Install()
        {
            IntegratedServiceInstaller integratedServiceInstaller = new IntegratedServiceInstaller();
            integratedServiceInstaller.Install(m_ServiceController.ServiceName, m_ServiceController.ServiceTitle, m_ServiceController.ServiceDescription,
                System.ServiceProcess.ServiceAccount.LocalSystem,
                System.ServiceProcess.ServiceStartMode.Automatic);

        }

        void Uninstall()
        {
            IntegratedServiceInstaller integratedServiceInstaller = new IntegratedServiceInstaller();
            integratedServiceInstaller.Uninstall(m_ServiceController.ServiceName);
        }

        class IntegratedServiceInstaller
        {
            public void Install(String ServiceName, String DisplayName, String Description,
                System.ServiceProcess.ServiceAccount Account,
                System.ServiceProcess.ServiceStartMode StartMode)
            {
                System.ServiceProcess.ServiceProcessInstaller ProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
                ProcessInstaller.Account = Account;

                System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

                System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext();
                string processPath = Process.GetCurrentProcess().MainModule.FileName;
                if (processPath != null && processPath.Length > 0)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(processPath);

                    String path = String.Format("/assemblypath={0}", fi.FullName);
                    String[] cmdline = { path };
                    Context = new System.Configuration.Install.InstallContext("", cmdline);
                }

                SINST.Context = Context;
                SINST.DisplayName = DisplayName;
                SINST.Description = Description;
                SINST.ServiceName = ServiceName;
                SINST.StartType = StartMode;
                SINST.Parent = ProcessInstaller;

                System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
                SINST.Install(state);

                using (RegistryKey oKey = Registry.LocalMachine.OpenSubKey(String.Format(@"SYSTEM\CurrentControlSet\Services\{0}", SINST.ServiceName), true))
                {
                    try
                    {
                        Object sValue = oKey.GetValue("ImagePath");
                        oKey.SetValue("ImagePath", sValue);
                    }
                    catch (Exception Ex)
                    {

                    }
                }

            }

            public void Uninstall(String ServiceName)
            {
                System.ServiceProcess.ServiceInstaller SINST = new System.ServiceProcess.ServiceInstaller();

                System.Configuration.Install.InstallContext Context = new System.Configuration.Install.InstallContext("c:\\install.log", null);
                SINST.Context = Context;
                SINST.ServiceName = ServiceName;
                SINST.Uninstall(null);
            }
        }
    }
}
