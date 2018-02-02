using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Runner.Reporters
{
    public class VstsClient
    {
        readonly IRunnerLogger logger;
        readonly string baseUri;
        readonly int buildId;
        static readonly HttpMethod PatchHttpMethod = new HttpMethod("PATCH");

        public VstsClient(IRunnerLogger logger, string baseUri, string accessToken, int buildId)
        {
            this.logger = logger;
            this.baseUri = baseUri;
            this.buildId = buildId;
            client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken); 

            workTask = Task.Run(RunLoop);
        }

        readonly HttpClient client;
        readonly MediaTypeWithQualityHeaderValue jsonMediaType = new MediaTypeWithQualityHeaderValue("application/json");

        readonly Task workTask;
        volatile bool previousErrors;

        readonly ManualResetEventSlim finished = new ManualResetEventSlim(false);
        readonly ManualResetEventSlim workEvent = new ManualResetEventSlim(false);
        volatile bool shouldExit;

        ConcurrentQueue<IDictionary<string, object>> addQueue = new ConcurrentQueue<IDictionary<string, object>>();

        ConcurrentQueue<IDictionary<string, object>> updateQueue = new ConcurrentQueue<IDictionary<string, object>>();

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
            int? runId = null;
            try
            {
                runId = await CreateTestRun();

                while (!shouldExit || !addQueue.IsEmpty || !updateQueue.IsEmpty)
                {
                    workEvent.Wait();   // Wait for work
                    workEvent.Reset();  // Reset first to ensure any subsequent modification sets

                    // Get local copies of the queues
                    var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<IDictionary<string, object>>());
                    var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<IDictionary<string, object>>());

                    if (previousErrors)
                        break;

                    // We have to do add's before update because we need the test id from the add to inject into the update
                    await SendTestResults(HttpMethod.Post, runId.Value, aq.ToArray()).ConfigureAwait(false);
               //     await SendTestResults(PatchHttpMethod, runId.Value, uq.ToArray()).ConfigureAwait(false);
                }
            }
            catch { }
            finally
            {
                try
                {
                    if (runId.HasValue)
                        await FinishTestRun(runId.Value);
                }
                catch
                {  }
                finished.Set();
            }
        }


        public void AddTest(IDictionary<string, object> request)
        {
            addQueue.Enqueue(request);
            workEvent.Set();
        }

        public void UpdateTest(IDictionary<string, object> request)
        {
            updateQueue.Enqueue(request);
            workEvent.Set();
        }

        async Task<int> CreateTestRun()
        {
            var requestMessage = new Dictionary<string, object>
            {
                { "name", "xUnit Runner Test Run"},
                {
                    "build", 
                    new Dictionary<string, object>
                    {
                        { "id", buildId }
                    }
                },
                { "isAutomated", true }
            };

            var bodyString = requestMessage.ToJson();
            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUri}?api-version=1.0")
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
                        logger.LogWarning($"When sending 'POST {baseUri}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        previousErrors = true;
                    }


                    using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()
                                                                       .ConfigureAwait(false)))
                    {
                        var resp = JsonDeserializer.Deserialize(reader) as JsonObject;
                        var id = resp.ValueAsInt("id");
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending 'POST {baseUri}' with body '{bodyString}', exception was thrown: {ex.Message}");
                throw;
            }
        }

        async Task FinishTestRun(int testRunId)
        {
            var requestMessage = new Dictionary<string, object>
            {
                { "completedDate", DateTime.UtcNow },
                { "state", "Completed" }
            };

            var bodyString = requestMessage.ToJson();
            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(PatchHttpMethod, $"{baseUri}/{testRunId}?api-version=1.0")
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
                        logger.LogWarning($"When sending 'PATCH {baseUri}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        previousErrors = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending 'PATCH {baseUri}' with body '{bodyString}', exception was thrown: {ex.Message}");
                throw;
            }
        }


        async Task SendTestResults(HttpMethod method, int runId, ICollection<IDictionary<string, object>> body)
        {
            if (body.Count == 0)
                return;

            var bodyString = ToJson(body);

            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(method, $"{baseUri}/{runId}/results?api-version=3.0-preview")
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


        static string ToJson(IEnumerable<IDictionary<string, object>> data)
        {
            var results = string.Join(",", data.Select(x => x.ToJson()));
            return $"[{results}]";
        }
    }
}
