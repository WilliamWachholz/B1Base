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

            var dao = (DAO.BaseDAO<T>) Activator.CreateInstance(daoType);

            return dao.Get(code);
        }

        public void Save<T>(T model) where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

            dao.Save(model);
        }


        public void Delete<T>(T model) where T : Model.BaseModel
        {
            Type modelType = typeof(T);

            Type daoType = Type.GetType(modelType.AssemblyQualifiedName.Replace("Model", "DAO"));

            var dao = (DAO.BaseDAO<T>)Activator.CreateInstance(daoType);

            dao.Delete(model);
        }
    }
}
