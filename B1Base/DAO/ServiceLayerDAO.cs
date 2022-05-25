using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace B1Base.DAO
{
    public class ServiceLayerDAO
    {
        private string BaseUrl
        {
            get
            {
                return string.Format("https://{0}/b1s/v1/", B1Base.AddOn.Instance.ConnectionController.Company.LicenseServer.Replace("40000", "50000"));
            }
        }

        private string Cookies
        {
            get; set;
        }

        public void Login()
        {
            Model.LoginEntity loginEntity = new Model.LoginEntity();
            loginEntity.CompanyDB = "SBOBLUTRADE";
            loginEntity.UserName = "manager";
            loginEntity.Password = "Sos1.";

            string data = ConvertToJsonString(loginEntity);

            string url = BaseUrl + "Login";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { streamWriter.Write(data); }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Console.WriteLine(result);
                Model.LoginResponseEntity loginResponseEntity = JsonConvert.DeserializeObject<Model.LoginResponseEntity>(result);

                Cookies = "B1SESSION=" + loginResponseEntity.SessionId + "; ROUTEID=.node4";
            }
        }

        public string SendPATCH(string url, string data)
        {
            string result = string.Empty;

            url = BaseUrl + url;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { streamWriter.Write(data); }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public string SendPOST(string url, string data)
        {
            string result = string.Empty;

            url = BaseUrl + url;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { streamWriter.Write(data); }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public string SendPOST(string url, string data, string batch)
        {
            string result = string.Empty;

            url = BaseUrl + url;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "multipart/mixed;boundary=" + batch;
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { streamWriter.Write(data); }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public string GenerateBatchID()
        {
            return string.Format("batch_{0}", Guid.NewGuid().ToString());
        }

        public string MakePOSTBatchContent(string[] data, string[] url, string batch)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(string.Format("--{0}", batch));
            result.AppendLine("Content-Type: application/http");
            result.AppendLine("Content-Transfer-Encoding:binary");

            for (int contentID = 0; contentID < data.Count(); contentID++)
            {
                result.AppendLine();

                string changeset = string.Format("changeset_{0}", Guid.NewGuid().ToString());

                result.AppendLine(string.Format("Content-Type: multipart/mixed;boundary={0}", changeset));
                result.AppendLine("Content-Type: application/http");
                result.AppendLine("Content-Transfer-Encoding:binary");
                result.AppendLine(string.Format("Content-ID: {0}", contentID + 1));

                result.AppendLine();

                result.AppendLine(string.Format("POST /b1s/v1/{0}", url[contentID]));

                result.AppendLine();

                result.AppendLine(data[contentID]);

                result.AppendLine();

                result.AppendLine(string.Format("--{0}--", changeset));                
            }

            result.AppendLine(string.Format("--{0}--", batch));

            return result.ToString();
        }

        public static T ConvertFromJsonString<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                DateFormatString = "yyyy-MM-dd"
            });
        }

        public static string ConvertToJsonString(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                DateFormatString = "yyyy-MM-dd"
            });
        }
    }
}
