using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace B1Base.DAO
{
    public class ServiceLayerDAO
    {
        private string BaseUrl
        {
            get
            {
                return string.Format("https://{0}/b1s/v1/", B1Base.Controller.ConnectionController.Instance.Company.LicenseServer.Replace("40000", "50000"));
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

        public void Logout()
        {
            SendPOST("Logout");
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

        public string SendPOST(string url)
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

        public int PostEntity(object obj, string entityName, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(ConvertToJsonString(obj, contractResolver));
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            if (httpResponse.StatusCode == (HttpStatusCode) 204)
            {
                string location = httpResponse.Headers.Get("Location");

                int id = Convert.ToInt32(location.Substring(location.IndexOf("(") + 1,
                        location.IndexOf(")") - location.IndexOf("(") - 1));

                return id;
            }
            else
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string resultContent = streamReader.ReadToEnd();

                    string messageJson = "";

                    dynamic jobj = JObject.Parse(resultContent);

                    messageJson = jobj.error.message.value;

                    throw new Exception(messageJson);
                }
            }
        }

        public void PatchEntity(object obj, string entityName, int entityID, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName + "(" + entityID.ToString() + ")");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(ConvertToJsonString(obj, contractResolver));
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            if (httpResponse.StatusCode != (HttpStatusCode)204)
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string resultContent = streamReader.ReadToEnd();

                    string messageJson = "";

                    dynamic jobj = JObject.Parse(resultContent);

                    messageJson = jobj.error.message.value;

                    throw new Exception(messageJson);
                }
            }
        }

        public void DeleteEntity(string entityName, string entityCode, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName + "(" + entityCode + ")");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "DELETE";
            httpWebRequest.KeepAlive = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            if (httpResponse.StatusCode != (HttpStatusCode)204)
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string resultContent = streamReader.ReadToEnd();

                    string messageJson = "";

                    dynamic jobj = JObject.Parse(resultContent);

                    messageJson = jobj.error.message.value;

                    throw new Exception(messageJson);
                }
            }
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

        public string MakeBatchContent(string[] data, string[] url, string[] verbs, string batch)
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

                result.AppendLine(string.Format("{0} /b1s/v1/{1}", verbs[contentID], url[contentID]));

                result.AppendLine();

                if (data[contentID] != "")
                {
                    result.AppendLine(data[contentID]);

                    result.AppendLine();
                }

                result.AppendLine(string.Format("--{0}--", changeset));
            }

            result.AppendLine(string.Format("--{0}--", batch));

            return result.ToString();
        }

        public void SendBatch(List<Model.SLBatchModel> slBatchList)
        {
            string batchContent = "";

            string batchResult = "";

            SendBatch(slBatchList, out batchContent, out batchResult);
        }
        
        public void SendBatch(List<Model.SLBatchModel> slBatchList, out string batchContent, out string batchResult)
        {
            string batchID = GenerateBatchID();

            List<string> data = new List<string>();
            List<string> verbs = new List<string>();
            List<string> url = new List<string>();

            foreach (Model.SLBatchModel slBatchModel in slBatchList)
            {
                data.Add(slBatchModel.Content);
                verbs.Add(slBatchModel.Verb);
                url.Add(slBatchModel.Path);
            }

            batchContent = MakeBatchContent(data.ToArray(), url.ToArray(), verbs.ToArray(), batchID);

            batchResult = SendPOST("$batch", batchContent, batchID);

            string[] details = batchResult.Split(new string[] { "HTTP/1.1" }, StringSplitOptions.None);

            for (int pos = 1; pos < details.Count(); pos++)
            {
                Model.SLBatchModel slBatchModel = slBatchList[pos - 1];

                string detail = details[pos];

                List<string> content = new StringBuilder(detail).ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                int code = Convert.ToInt32(content[0].Substring(1, 3));

                if (code == 201)
                {
                    int locationIndex = 0;

                    foreach (string text in content)
                    {
                        if (text.Contains("Location"))
                            break;
                        locationIndex++;
                    }

                    slBatchModel.ResultEntityCode = content[locationIndex].Substring(content[locationIndex].IndexOf("(") + 1,
                        content[locationIndex].IndexOf(")") - content[locationIndex].IndexOf("(") - 1);

                    slBatchModel.ResultEntityCode = slBatchModel.ResultEntityCode.Replace("'", "");

                    int id;

                    if (int.TryParse(slBatchModel.ResultEntityCode, out id))
                        slBatchModel.ResultEntityId = id;
                }
                else if (code == 204)
                {

                }
                else
                {
                    string messageJson = "";

                    foreach (string text in content)
                    {
                        try
                        {
                            dynamic jobj = JObject.Parse(text);

                            messageJson = jobj.error.message.value;

                            break;
                        }
                        catch { }
                    }

                    if (messageJson == "")
                    {
                        messageJson = content[0].Substring(4).Trim();
                    }

                    slBatchModel.ResultError = true;

                    slBatchModel.ResultMessage = messageJson;
                }
            }

        }

        public static T ConvertFromJsonString<T>(string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver(),
                DateFormatString = "yyyy-MM-dd"
            });
        }

        public static string ConvertToJsonString(object obj, IContractResolver contractResolver = null)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = contractResolver == null ? new DefaultContractResolver() : contractResolver,
                DateFormatString = "yyyy-MM-dd"
            });
        }
    }
}
