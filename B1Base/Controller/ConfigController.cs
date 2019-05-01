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
            try
            {
                return m_ConfigDAO.Get(1);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }

            return new Model.ConfigModel() as T;
        }

        public void SaveConfig(Model.ConfigModel configModel)
        {
            try
            {
                m_ConfigDAO.Save(configModel as T);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }
        }

        public List<Model.ConfigSeqModel> GetListConfigSeq()
        {
            return m_ConfigSeqDAO.GetList();
        }

        public void SaveConfigSeq(List<Model.ConfigSeqModel> configSeqList)
        {
            try
            {
                Save<Model.ConfigSeqModel>(GetListConfigSeq(), configSeqList);
            }
            catch (Exception e)
            {
                Controller.ConnectionController.Instance.Application.StatusBar.SetText(e.Message);
            }
        }
    }
}
