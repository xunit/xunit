using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if PLATFORM_DOTNET
using Newtonsoft.Json;
#else
using System.Web.Script.Serialization;
#endif

namespace Xunit.Runner.Reporters
{
    public class AppVeyorClient
    {
        readonly IRunnerLogger logger;
        readonly string baseUri;

        public AppVeyorClient(IRunnerLogger logger, string baseUri)
        {
            this.logger = logger;
            this.baseUri = $"{baseUri}/api/tests/batch";

            workTask = Task.Run(RunLoop);
        }

        readonly HttpClient client = new HttpClient();
        readonly MediaTypeWithQualityHeaderValue jsonMediaType = new MediaTypeWithQualityHeaderValue("application/json");

        readonly Task workTask;
        volatile bool previousErrors;

        readonly ManualResetEventSlim finished = new ManualResetEventSlim(false);
        readonly ManualResetEventSlim workEvent = new ManualResetEventSlim(false);
        volatile bool shouldExit;

        ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest> addQueue = new ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest>();

        ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest> updateQueue = new ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest>();

        public void WaitOne(CancellationToken cancellationToken)
        {
            // Free up to process any remaining work
            shouldExit = true;
            workEvent.Set();

            finished.Wait(cancellationToken);
            finished.Dispose();
        }

        async Task RunLoop()
        {
            try
            {
                while (!shouldExit || !addQueue.IsEmpty || !updateQueue.IsEmpty)
                {
                    workEvent.Wait();   // Wait for work
                    workEvent.Reset();  // Reset first to ensure any subsequent modification sets

                    // Get local copies of the queues
                    var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest>());
                    var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<AppVeyorReporterMessageHandler.AddUpdateTestRequest>());

                    if (previousErrors)
                        break;

                    await Task.WhenAll(
                        SendRequest(HttpMethod.Post, aq.ToArray()),
                        SendRequest(HttpMethod.Put, uq.ToArray())
                    ).ConfigureAwait(false);
                }
            }
            catch { }
            finally
            {
                finished.Set();
            }
        }


        public void AddTest(AppVeyorReporterMessageHandler.AddUpdateTestRequest request)
        {
            addQueue.Enqueue(request);
            workEvent.Set();
        }

        public void UpdateTest(AppVeyorReporterMessageHandler.AddUpdateTestRequest request)
        {
            updateQueue.Enqueue(request);
            workEvent.Set();
        }

        async Task SendRequest(HttpMethod method, ICollection<AppVeyorReporterMessageHandler.AddUpdateTestRequest> body)
        {
            if (body.Count == 0)
                return;

            var bodyString = ToJson(body);

            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(method, baseUri)
                {
                    Content = new ByteArrayContent(bodyBytes)
                };
                request.Content.Headers.ContentType = jsonMediaType;
                request.Headers.Accept.Add(jsonMediaType);

                using (var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogWarning($"When sending '{method} {baseUri}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        previousErrors = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending '{method} {baseUri}' with body '{bodyString}', exception was thrown: {ex.Message}");
                throw;
            }
        }


        static string ToJson(IEnumerable<AppVeyorReporterMessageHandler.AddUpdateTestRequest> data)
        {
#if PLATFORM_DOTNET
            return JsonConvert.SerializeObject(data);
#else
            return new JavaScriptSerializer().Serialize(data);
#endif
        }
    }
}
