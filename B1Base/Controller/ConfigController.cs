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


        public ConfigController(string configTable)
        {
            if (configTable == "")
                configTable = Controller.ConnectionController.Instance.AddOnID + "Cnf";

            m_ConfigDAO = new DAO.ConfigDAO<T>(configTable);
            m_ConfigSeqDAO = new DAO.ConfigSeqDAO();
        }

        public void CreateMetadata(string field, FieldTypeEnum fieldType, string fieldTitle)
        {
            ConnectionController.Instance.CreateMetadata(m_ConfigDAO.TableName, field, fieldType, fieldTitle);
        }

        public void CreateMetadata(string field, FieldTypeEnum fieldType, int size = 10, Dictionary<string, string> validValues = null, string defaultValue = "", string fieldTitle = "")
        {
            ConnectionController.Instance.CreateMetadata(m_ConfigDAO.TableName, field, fieldType, size, validValues, defaultValue, fieldTitle);
        }

        public T GetConfig()
        {
            Type modelType = typeof(T);

            if (Controller.ConnectionController.Instance.ODBCConnection)
            {
                var model = (T)Activator.CreateInstance(modelType);

                List<string> fieldList = Controller.ConnectionController.Instance.ExecuteSqlForList<string>("GetListField", m_ConfigDAO.TableName);

                string fields = string.Join(",", fieldList.ToArray());

                return Controller.ConnectionController.Instance.ExecuteSqlForObject<T>("GetModel", m_ConfigDAO.TableName, fields, 1.ToString());
            }
            else
            {
                Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

                if (daoType == null)
                    return m_ConfigDAO.Get(1);
                else
                {
                    var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

                    return dao.Get(1);
                }
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
