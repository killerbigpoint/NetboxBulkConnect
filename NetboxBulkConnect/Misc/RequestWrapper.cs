using System.Net.Security;
using System.Text;
using System.Net;
using System.IO;

namespace NetboxBulkConnect.Misc
{
    public class RequestWrapper
    {
        public enum RetrieveType : int
        {
            GET = 0,
            OPTIONS = 1
        }

        public struct RequestResponse
        {
            public HttpStatusCode statusCode;
            public string data;
        }

        public static string GetHttpPrefix()
        {
            return Config.GetConfig().UseHttpEncryption == true ? "https://" : "http://";
        }

        public static string GetServer()
        {
            return $"{GetHttpPrefix()}{Config.GetConfig().Server}";
        }

        public static void InitializeWebClient()
        {
            //Change SSL checks so that all checks pass
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate
            {
                return true;
            });

            FileLogging.Append("HTTP Initialized");
        }

        public static RequestResponse RetrieveRequest(string endpoint, RetrieveType type)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{GetServer()}/api/{endpoint}");
            request.Headers.Add("Authorization", $"Token {Config.GetConfig().ApiToken}");
            request.Method = type.ToString();

            HttpWebResponse webResponse;
            StreamReader webReader;
            try
            {
                webResponse = (HttpWebResponse)request.GetResponse();
                webReader = new StreamReader(webResponse.GetResponseStream());
            }
            catch (WebException ex)
            {
                webResponse = (HttpWebResponse)ex.Response;
                webReader = new StreamReader(webResponse.GetResponseStream());
            }

            RequestResponse response = new RequestResponse()
            {
                statusCode = webResponse.StatusCode,
                data = webReader.ReadToEnd()
            };

            return response;
        }

        public static HttpStatusCode PostRequest(string endpoint, string data)
        {
            byte[] postData = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{GetServer()}/api/{endpoint}");
            request.Headers.Add("Authorization", $"Token {Config.GetConfig().ApiToken}");
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json; charset=utf-8";
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(postData, 0, postData.Length);
            }

            HttpStatusCode responseCode;
            try
            {
                responseCode = ((HttpWebResponse)request.GetResponse()).StatusCode;
            }
            catch (WebException ex)
            {
                responseCode = ((HttpWebResponse)ex.Response).StatusCode;
            }

            return responseCode;
        }
    }
}
