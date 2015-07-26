using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Xunit.Runner.Reporters
{
    public static class AppVeyorClient
    {
        static string apiUrl = "__unknown__";
        static HttpClient httpClient;
        static readonly MediaTypeWithQualityHeaderValue jsonMediaType = new MediaTypeWithQualityHeaderValue("application/json");

        static string ApiUri
        {
            get
            {
                if (apiUrl == "__unknown__")
                    apiUrl = GetApiUri();

                return apiUrl;
            }
        }

        static HttpClient HttpClient
        {
            get
            {
                if (httpClient == null)
                    httpClient = new HttpClient { BaseAddress = new Uri(ApiUri) };

                return httpClient;
            }
        }

        public static bool IsRunningInAppVeyor
        {
            get { return ApiUri != null; }
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
            var httpMethod = new HttpMethod(method);
            var bodyString = JsonConvert.SerializeObject(body);
            var bodyBytes = Encoding.UTF8.GetBytes(bodyString);
            var content = new ByteArrayContent(bodyBytes);
            content.Headers.ContentType = jsonMediaType;
            var request = new HttpRequestMessage(httpMethod, url) { Content = content };
            request.Headers.Accept.Add(jsonMediaType);
            var response = HttpClient.SendAsync(request).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
}
