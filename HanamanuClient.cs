using System;
using System.IO;
using System.Net;
using System.Text;

namespace Hanamanu
{
    public class HanamanuClient
    {
        public class HanamanuClientException : ApplicationException
        {
            public HanamanuClientException(string message)
                : base(message)
            {

            }

            public HanamanuClientException(string message, Exception innerException)
                : base(message, innerException)
            {

            }
        }

        private const string HANAMANU_END_POINT = "http://api.hanamanu.com/v1/events";
        private const string HTTP_METHOD_POST = "POST";
        private const string REQUEST_CONTENT_TYPE = "application/json";
        private const string AUHTHORIZATION_HEADER_NAME = "Authorization";

        private static Encoding ASCII = ASCIIEncoding.ASCII;
        private static Encoding UTF8 = UTF8Encoding.UTF8;

        private static byte[] REQUEST_PART_1 = ASCII.GetBytes("{\"MetricName\":\"");
        private static byte[] REQUEST_PART_2 = ASCII.GetBytes("\",\"Value\":");
        private static byte[] REQUEST_PART_3 = ASCII.GetBytes("}");

        private string appSecretKey;

        public HanamanuClient(string appSecretKey)
        {
            if (String.IsNullOrWhiteSpace(appSecretKey)) throw new ArgumentNullException("appSecretKey");

            this.appSecretKey = appSecretKey;
        }

        public void SendEvent(string metricName, decimal value)
        {
            if (String.IsNullOrWhiteSpace(metricName)) throw new ArgumentNullException("metricName");

            WebRequest request = null;
            WebResponse response = null;

            try
            {
                byte[] bytesMetricName = UTF8.GetBytes(metricName);
                byte[] bytesValue = ASCII.GetBytes(value.ToString());

                request = HttpWebRequest.Create(HANAMANU_END_POINT);

                request.Method = HTTP_METHOD_POST;
                request.ContentType = REQUEST_CONTENT_TYPE;

                request.Headers[AUHTHORIZATION_HEADER_NAME] = this.appSecretKey;

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(REQUEST_PART_1, 0, REQUEST_PART_1.Length);
                    stream.Write(bytesMetricName, 0, bytesMetricName.Length);
                    stream.Write(REQUEST_PART_2, 0, REQUEST_PART_2.Length);
                    stream.Write(bytesValue, 0, bytesValue.Length);
                    stream.Write(REQUEST_PART_3, 0, REQUEST_PART_3.Length);
                }

                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    HttpWebResponse invalidResponse = (HttpWebResponse)ex.Response;

                    throw new HanamanuClientException(String.Format("Hanamanu server returned error: {0} -> {1}", invalidResponse.StatusCode, invalidResponse.StatusDescription), ex);
                }
                else
                {
                    throw new HanamanuClientException("Hanamanu Client Error!", ex);
                }
            }
            catch (Exception ex)
            {
                throw new HanamanuClientException("Hanamanu Client Error!", ex);
            }
        }
    }
}
