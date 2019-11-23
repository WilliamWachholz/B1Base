using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Controller
{
    public class ConfigController<T> : BaseController where T : Model.ConfigModel
    {
        DAO.ConfigDAO<T> m_ConfigDAO;
        DAO.ConfigSeqDAO m_ConfigSeqDAO;

        public ConfigController()
        {
            m_ConfigDAO = new DAO.ConfigDAO<T>();
            m_ConfigSeqDAO = new DAO.ConfigSeqDAO();
        }

        public T GetConfig()
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            if (daoType == null)
                return m_ConfigDAO.Get(1);
            else
            {
                var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

                return dao.Get(1);
            }
        }

        public void SaveConfig(Model.ConfigModel configModel)
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            if (daoType == null)
                m_ConfigDAO.Save(configModel as T);
            else
            {
                var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

                dao.Save(configModel as T);
            }            
        }

        public List<Model.ConfigSeqModel> GetListConfigSeq()
        {
            return m_ConfigSeqDAO.GetList();
        }

        public void SaveConfigSeq(List<Model.ConfigSeqModel> configSeqList)
        {
            Save<Model.ConfigSeqModel>(GetListConfigSeq(), configSeqList);
        }
    }
}
