using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    class ConfigDAO<T> : BaseDAO<T> where T : Model.ConfigModel
    {
        protected override string TableName
        {
            get
            {
                return (Controller.ConnectionController.Instance.AddOnID + "Cnf").ToUpper();
            }
        }
    }
}
