using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Controller
{
    public class BaseController
    {
        public T Get<T>(int code) where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            var model = (T)Activator.CreateInstance(modelType);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

            return dao.Get(code);
        }

        public List<T> Get<T>() where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            var model = (T)Activator.CreateInstance(modelType);

            return AddOn.Instance.ConnectionController.ExecuteSqlForList<T>("GetListModel", modelType.AssemblyQualifiedName.Replace("Model", ""));
        }

        public void Save<T>(T model) where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

            dao.Save(model);
        }

        public void Save<T>(List<T> listSource, List<T> listCurrent) where T : Model.BaseModel
        {
            for (int i = 0; i < listCurrent.Count; i++)
            {
                if (listSource.Where(r => r.Code == listCurrent[i].Code).Count() > 0)
                {
                    if (listCurrent[i].Code == 0)
                    {
                        Save<T>(listCurrent[i]);
                    }
                    else
                    {
                        Compare(listSource.First(r => r.Code == listCurrent[i].Code), listCurrent[i]);

                        if (listCurrent[i].Changed)
                        {
                            Save<T>(listCurrent[i]);
                        }
                    }
                }
                else
                {
                    Save<T>(listCurrent[i]);
                }
            }

            for (int i = 0; i < listSource.Count; i++)
            {
                if (listCurrent.Where(r => r.Code == listSource[i].Code).Count() == 0)
                {
                    Delete<T>(listSource[i]);
                }
            }
        }

        public void Delete<T>(T model) where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

            dao.Delete(model);
        }

        private void Compare<T>(T source, T current) where T : Model.BaseModel
        {
            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            foreach (var prop in props)
            {
                if (prop.GetValue(source) != prop.GetValue(current))
                {
                    current.Changed = true;
                    break;
                }
            }
        }
    }
}
