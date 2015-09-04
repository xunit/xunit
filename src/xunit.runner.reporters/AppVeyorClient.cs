using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Xunit.Sdk;

#if PLATFORM_DOTNET
using Newtonsoft.Json;
#else
using System.Web.Script.Serialization;
#endif

namespace Xunit.Runner.Reporters
{
    public static class AppVeyorClient
    {
        static readonly HttpClient client = new HttpClient();
        static readonly MediaTypeWithQualityHeaderValue jsonMediaType = new MediaTypeWithQualityHeaderValue("application/json");

        public static void SendRequest(IRunnerLogger logger, string url, string method, object body)
        {
            using (var finished = new ManualResetEvent(false))
            {
                XunitWorkerThread.QueueUserWorkItem(async () =>
                {
                    try
                    {
                        var bodyString = ToJson(body);
                        var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                        var request = new HttpRequestMessage(HttpMethod.Post, url);
                        request.Content = new ByteArrayContent(bodyBytes);
                        request.Content.Headers.ContentType = jsonMediaType;
                        request.Headers.Accept.Add(jsonMediaType);

                        using (var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                        {
                            var response = await client.SendAsync(request, tcs.Token);
                            if (!response.IsSuccessStatusCode)
                                logger.LogWarning($"When sending '{method} {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"When sending '{method} {url}', exception was thrown: {ex}");
                    }
                }, finished);

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
