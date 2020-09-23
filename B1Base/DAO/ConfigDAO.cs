using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    public class ConfigDAO<T> : BaseDAO<T> where T : Model.ConfigModel
    {
        protected override string TableName
        {
            get
            {
                return Controller.ConnectionController.Instance.ConfigTableName == "" ?
                    (Controller.ConnectionController.Instance.AddOnID + "Cnf").ToUpper():
                    Controller.ConnectionController.Instance.ConfigTableName;
            }
        }
    }
}
