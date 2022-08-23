using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1Base.Model
{
    public class SLBatchModel
    {
        public string Verb { get; private set; }

        public string Content { get; private set; }

        public string Path { get; private set; }

        public bool ResultError { get; set; }

        public string ResultMessage { get; set; }

        public int ResultEntityId { get; set; }

        public string ResultEntityCode { get; set; }

        public static SLBatchModel PostMethod(string method, IContractResolver contractResolver = null)
        {
            SLBatchModel result = new SLBatchModel();
            result.Verb = "POST";
            result.Path = method;
            result.Content = "";

            return result;
        }

        public static SLBatchModel PostEntity(object obj, string entityName, IContractResolver contractResolver = null)
        {
            SLBatchModel result = new SLBatchModel();
            result.Verb = "POST";
            result.Path = entityName;
            result.Content = DAO.ServiceLayerDAO.ConvertToJsonString(obj, contractResolver);

            return result;
        }

        public static SLBatchModel PatchEntity(object obj, string entityName, int id, IContractResolver contractResolver = null)
        {
            SLBatchModel result = new SLBatchModel();
            result.Verb = "PATCH";
            result.Path = entityName + "(" + id.ToString() + ")";
            result.Content = DAO.ServiceLayerDAO.ConvertToJsonString(obj, contractResolver);

            return result;
        }

        public static SLBatchModel DeleteEntity(string entityName, string code, IContractResolver contractResolver = null)
        {
            SLBatchModel result = new SLBatchModel();
            result.Verb = "DELETE";
            result.Path = entityName + "('" + code + "')";
            
            return result;
        }

        public static SLBatchModel DeleteEntity(string entityName, int id, IContractResolver contractResolver = null)
        {
            SLBatchModel result = new SLBatchModel();
            result.Verb = "DELETE";
            result.Path = entityName + "(" + id.ToString() + ")";

            return result;
        }
    }
}
