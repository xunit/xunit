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
        static volatile bool previousErrors;

        public static void SendRequest(IRunnerLogger logger, string url, HttpMethod method, object body)
        {
            if (previousErrors)
                return;

            lock (jsonMediaType)
            {
                using (var finished = new ManualResetEvent(false))
                {
                    XunitWorkerThread.QueueUserWorkItem(async () =>
                    {
                        var bodyString = ToJson(body);

                        try
                        {
                            var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                            var request = new HttpRequestMessage(method, url);
                            request.Content = new ByteArrayContent(bodyBytes);
                            request.Content.Headers.ContentType = jsonMediaType;
                            request.Headers.Accept.Add(jsonMediaType);

                            using (var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                            {
                                var response = await client.SendAsync(request, tcs.Token);
                                if (!response.IsSuccessStatusCode)
                                {
                                    logger.LogWarning($"When sending '{method} {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
                                    previousErrors = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"When sending '{method} {url}' with body '{bodyString}', exception was thrown: {ex.Message}");
                            previousErrors = true;
                        }
                        finally
                        {
                            finished.Set();
                        }
                    });

                    finished.WaitOne();
                }
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
