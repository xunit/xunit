using System.Net;
using System.Text;
using System.Threading;

#if PLATFORM_DOTNET
using Newtonsoft.Json;
#else
using System.Web.Script.Serialization;
#endif

namespace Xunit.Runner.Reporters
{
    public static class AppVeyorClient
    {
        public static void SendRequest(IRunnerLogger logger, string url, string method, object body)
        {
            using (var finished = new ManualResetEvent(false))
            {
                var request = WebRequest.CreateHttp(url);
                request.Method = method;
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.BeginGetRequestStream(requestResult =>
                {
                    var bodyString = ToJson(body);
                    var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                    using (var postStream = request.EndGetRequestStream(requestResult))
                    {
                        postStream.Write(bodyBytes, 0, bodyBytes.Length);
                        postStream.Flush();
                    }

                    request.BeginGetResponse(responseResult =>
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(responseResult);
                        if ((int)response.StatusCode >= 300)
                            logger.LogWarning($"When sending '{method} {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
                    }, null);
                }, null);

                finished.WaitOne();
            }
        }

        static string ToJson(object data)
        {
#if PLATFORM_DOTNET
            return JsonConvert.SerializeObject(data);
#else
            return new JavaScriptSerializer().Serialize(data);
#endif
        }
    }
}
