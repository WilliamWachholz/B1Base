using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
