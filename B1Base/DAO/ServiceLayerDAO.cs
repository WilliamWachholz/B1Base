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
                return string.Format("{0}/b1s/v1/", B1Base.Controller.ConnectionController.Instance.ServiceLayerURL);
            }
        }

        private string Cookies
        {
            get; set;
        }

        public void Login(string userName, string password)
        {
            Model.LoginEntity loginEntity = new Model.LoginEntity();
            if (Controller.ConnectionController.Instance.ODBCConnection)
                loginEntity.CompanyDB = B1Base.AddOn.Instance.ConnectionController.ODBCCompanyDB;
            else
                loginEntity.CompanyDB = B1Base.AddOn.Instance.ConnectionController.Company.CompanyDB;
            loginEntity.UserName = userName;
            loginEntity.Password = password;

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

                Cookies = "B1SESSION=" + loginResponseEntity.SessionId + "; ROUTEID=" + loginResponseEntity.RouteId;
            }
        }

        public void Logout()
        {
            SendPOST("Logout");
        }

        public string SendGET(string entityName, string entityID)
        {
            string result = string.Empty;

            string url = BaseUrl + entityName + "('" + entityID.ToString() + "')";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }

            return result;
        }

        public string SendPATCH(string url, string data)
        {
            string result = string.Empty;

            url = BaseUrl + url;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = false;
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

            string httpStatus = "";

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode == (HttpStatusCode)204)
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string resultContent = streamReader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                messageJson = "Não foi possível obter a resposta do servidor";
                            }

                            throw new Exception(httpStatus + ": " + messageJson);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                messageJson = "Não foi possível obter a resposta do servidor";
                            }

                            throw new Exception(httpStatus + ": " + messageJson);
                        }
                    }
                }
            }

            return result;
        }

        public void SendPATCH(string json, string entityName, string entityID, bool replaceCollections = false)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName + "('" + entityID.ToString() + "')");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            if (replaceCollections)
            {
                httpWebRequest.Headers.Add("B1S-ReplaceCollectionsOnPatch", "true");
            }
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
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
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            dynamic jobj = JObject.Parse(resultContent);

                            messageJson = jobj.error.message.value;

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
        }

        public string SendPOST(string url)
        {
            string result = string.Empty;

            url = BaseUrl + url;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;

            using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
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
            httpWebRequest.KeepAlive = false;
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

            string httpStatus = "";

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    httpStatus = ((int)httpResponse.StatusCode).ToString();

                    if (httpResponse.StatusCode == (HttpStatusCode)204 || httpResponse.StatusCode == (HttpStatusCode)201)
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            string resultContent = streamReader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                messageJson = "Não foi possível obter a resposta do servidor";
                            }

                            throw new Exception(httpStatus + ": " + messageJson);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                messageJson = "Não foi possível obter a resposta do servidor";
                            }

                            throw new Exception(httpStatus + ": " + messageJson);
                        }
                    }
                }
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
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Accept = "*/*";
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            httpWebRequest.Timeout = 1800000;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            { streamWriter.Write(data); }

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream stream = response.GetResponseStream())
                        using (var reader = new StreamReader(stream))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                throw new Exception(resultContent);
                            }
                        }
                    }
                }
            }        

            return result;
        }

        public int PostEntity(object obj, string entityName, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName);
            
            httpWebRequest.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            //httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "application/json;odata=minimalmetadata";
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            httpWebRequest.UserAgent = "b1base";
            httpWebRequest.Timeout = 600000;            

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(ConvertToJsonString(obj, contractResolver));
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode == (HttpStatusCode)204)
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

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                throw new Exception(resultContent);
                            }

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                throw new Exception(resultContent);
                            }

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
        }

        public int PostJson(string json, string entityName, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName);

            httpWebRequest.ContentType = "application/json;odata=minimalmetadata;charset=utf8";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServicePoint.Expect100Continue = false;
            //httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            httpWebRequest.Headers.Add("Cookie", Cookies);
            httpWebRequest.Headers.Add("Prefer", "return-no-content");
            httpWebRequest.Accept = "application/json;odata=minimalmetadata";
            httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip;
            httpWebRequest.UserAgent = "b1base";
            httpWebRequest.Timeout = 600000;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode == (HttpStatusCode)204)
                    {
                        string location = httpResponse.Headers.Get("Location");

                        if (location == null)
                            return 0;

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

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                throw new Exception(resultContent);
                            }

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            try
                            {
                                dynamic jobj = JObject.Parse(resultContent);

                                messageJson = jobj.error.message.value;
                            }
                            catch
                            {
                                throw new Exception(resultContent);
                            }

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
        }

        public string PostEntityString(object obj, string entityName, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
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

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpResponse.StatusCode == (HttpStatusCode)204)
                    {
                        string location = httpResponse.Headers.Get("Location");

                        string id = location.Substring(location.IndexOf("(") + 2,
                                location.IndexOf(")") - location.IndexOf("(") - 3);

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
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            dynamic jobj = JObject.Parse(resultContent);

                            messageJson = jobj.error.message.value;

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
        }

        public void PatchEntity(object obj, string entityName, int entityID, IContractResolver contractResolver = null)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName + "(" + entityID.ToString() + ")");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = false;
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

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
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
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    { 
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            dynamic jobj = JObject.Parse(resultContent);

                            messageJson = jobj.error.message.value;

                            throw new Exception(messageJson);
                        }
                    }
                }
            }
        }

        public void PatchEntityString(object obj, string entityName, string entityID, IContractResolver contractResolver = null, bool replaceCollections = false)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(BaseUrl + entityName + "('" + entityID.ToString() + "')");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PATCH";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            httpWebRequest.Headers.Add("B1S-WCFCompatible", "true");
            httpWebRequest.Headers.Add("B1S-MetadataWithoutSession", "true");
            if (replaceCollections)
            {
                httpWebRequest.Headers.Add("B1S-ReplaceCollectionsOnPatch", "true");
            }
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

            try
            {
                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
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
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    using (var httpResponse = (HttpWebResponse)response)
                    {
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string resultContent = reader.ReadToEnd();

                            string messageJson = "";

                            dynamic jobj = JObject.Parse(resultContent);

                            messageJson = jobj.error.message.value;

                            throw new Exception(messageJson);
                        }
                    }
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
