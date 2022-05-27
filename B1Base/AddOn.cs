using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPbouiCOM;
using System.Timers;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace B1Base
{
    /// <summary>
    /// Não usar essa classe para aplicativos externos que somente conectam pela DI. 
    /// Para esses casos utilizar a classe App
    /// </summary>
    public class AddOn
    {
        AddOn() { }

        static readonly AddOn m_Instance = new AddOn();

        public static AddOn Instance
        {
            get { return m_Instance; }
        }

        public Controller.MainController MainController { get; private set; }

        public Controller.ConnectionController ConnectionController { get { return Controller.ConnectionController.Instance; } }

        public void Initialize(Controller.MainController mainController)
        {
            this.MainController = mainController;
            this.MainController.Initialize();

            orgPID = getProcessID();

            System.Timers.Timer t = new System.Timers.Timer(60000);
            t.AutoReset = true;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        public string CurrentDirectory
        {
            get
            {
                if (ConnectionController.Desenv)
                    return Environment.GetCommandLineArgs().Count() > 2 ? Environment.GetCommandLineArgs().GetValue(2).ToString() : System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
                else
                    return System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);

            }
        }

        /// <summary>
        /// Inicia um programa contido na pasta (CurrentDirectory) do addOn. Informar a extensão
        /// </summary>
        /// <param name="file"></param>
        public void StartProcess(string file, bool stopRunningInstances = false)
        {
            if (stopRunningInstances)
            {
                foreach(System.Diagnostics.Process process in System.Diagnostics.Process.GetProcessesByName(file))
                {
                    process.Kill();
                }
            }
            System.Diagnostics.Process.Start(System.IO.Path.Combine(B1Base.AddOn.Instance.CurrentDirectory, file));
        }


        int orgPID { get; set; }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            var res = getProcessID();

            if (res != orgPID)
            {
                System.Windows.Forms.Application.Exit();
            }
        }


        private int getProcessID()
        {
            var gui = new SboGuiApi();
            try
            {
                gui.Connect("0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056");
            }
            catch (Exception)
            {
                return -1;
            }


            var appId = gui.GetApplication().AppId;

            var processes = Process.GetProcessesByName(@"SAP Business One").Where(x => x.SessionId == Process.GetCurrentProcess().SessionId);


            foreach (var process in processes)
            {
                try
                {
                    var processAppId = gui.GetAppIdFromProcessId(process.Id);
                    if (appId == processAppId)
                    {
                        return process.Id;
                    }
                }
                catch (COMException)
                {
                    // GetAppIdFromProcessId will throw when the current process is not the one we are connected to
                    continue;
                }
            }

            return -1;
        }
    }
}
