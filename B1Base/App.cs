using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base
{
    class App
    {
        App() { }

        static readonly App m_Instance = new App();

        public static App Instance
        {
            get { return m_Instance; }
        }

        public Controller.ConnectionController ConnectionController { get { return Controller.ConnectionController.Instance; } }

        public void Initialize()
        {

        }        
    }
}
