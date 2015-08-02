using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Xunit.Runner.Reporters
{
    public static class AppVeyorClient
    {
        static string apiUrl = "__unknown__";

        static string ApiUri
        {
            get
            {
                if (apiUrl == "__unknown__")
                    apiUrl = GetApiUri();

                return apiUrl;
            }
        }

        public static bool IsRunningInAppVeyor
            => ApiUri != null;

        static byte[] Json(object data)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }

        static WebClient GetClient()
        {
            var wc = new WebClient() { BaseAddress = ApiUri };
            wc.Headers["Accept"] = "application/json";
            wc.Headers["Content-type"] = "application/json";
            return wc;
        }

        static string GetApiUri()
        {
            var apiUrl = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

            if (apiUrl != null)
                apiUrl = apiUrl.TrimEnd('/') + "/";

            return apiUrl;
        }

        public static void SendRequest(string url, string method, object body)
        {
            using (var wc = GetClient())
                wc.UploadData(url, method, Json(body));
        }
    }
}
