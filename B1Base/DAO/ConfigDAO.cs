using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    public class ConfigDAO<T> : BaseDAO<T> where T : Model.ConfigModel
    {
        string m_ConfigTable = "";

        public ConfigDAO(string configTable)
        {
            m_ConfigTable = configTable;
        }

        public override string TableName
        {
            get
            {
                return m_ConfigTable;
            }
        }
    }
}
