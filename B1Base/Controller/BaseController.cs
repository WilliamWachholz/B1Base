using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Controller
{
    public abstract class BaseController
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

            List<string> fieldList = Controller.ConnectionController.Instance.ExecuteSqlForList<string>("GetListField", modelType.Name.Replace("Model", "").ToUpper());

            string fields = string.Join(",", fieldList.ToArray());

            return Controller.ConnectionController.Instance.ExecuteSqlForList<T>("GetListModel", modelType.Name.Replace("Model", "").ToUpper(), fields);
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
                    if (listSource[i].Code > 0)
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

        //atributo threadedMethod?

        private void Compare<T>(T source, T current) where T : Model.BaseModel
        {
            Type type = typeof(T);

            var props = type.GetProperties().Where(r => r.Name != "Changed");

            foreach (var prop in props)
            {
                if (!prop.GetValue(source).Equals(prop.GetValue(current)))
                {
                    current.Changed = true;
                    break;
                }
            }
        }

        public static string MsgSuccess
        {
            get { return "Operação Completada com êxito."; }
        }

        public static string MsgConfirmDelete
        {
            get { return "Deseja realmente eliminar esse registro? ";  }
        }

        public static string MsgConfirmAbort
        {
            get { return "Dados não salvos serão perdidos.";  }
        }        
    }
}
