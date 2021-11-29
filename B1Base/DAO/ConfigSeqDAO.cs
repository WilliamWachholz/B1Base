using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.DAO
{
    public class ConfigSeqDAO : BaseDAO<Model.ConfigSeqModel>
    {
        protected override string TableName
        {
            get
            {
                return AddOnSequenceTableName;
            }
        }        

        public static string AddOnSequenceTableName
        {
            get
            {
                //return (Controller.ConnectionController.Instance.AddOnID + "Seq").ToUpper();
                return Controller.ConnectionController.Instance.ConfigSeqTableName == "" ?
                    (Controller.ConnectionController.Instance.AddOnID + "Seq").ToUpper() :
                    Controller.ConnectionController.Instance.ConfigSeqTableName;
            }
        }

        public List<Model.ConfigSeqModel> GetList()
        {
            return Controller.ConnectionController.Instance.ExecuteSqlForList<Model.ConfigSeqModel>("GetListConfigSeq", TableName);
        }
    }
}
