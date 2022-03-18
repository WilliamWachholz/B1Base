using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    public class ConfigSeqDAO : BaseDAO<Model.ConfigSeqModel>
    {
        public ConfigSeqDAO()
        {
            
        }

        public override string TableName
        {
            get
            {
                return Controller.ConnectionController.Instance.AddOnID.ToUpper() + "SEQ";
            }
        }
        

        public List<Model.ConfigSeqModel> GetList()
        {
            return Controller.ConnectionController.Instance.ExecuteSqlForList<Model.ConfigSeqModel>("GetListConfigSeq", TableName);
        }
    }
}
