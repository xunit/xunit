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
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class VstsClient : IDisposable
    {
        readonly IRunnerLogger logger;
        readonly string baseUri;
        readonly int buildId;
        static readonly HttpMethod PatchHttpMethod = new HttpMethod("PATCH");
        const string UNIQUEIDKEY = "UNIQUEIDKEY";

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
        readonly AutoResetEvent workEvent = new AutoResetEvent(false);
        volatile bool shouldExit;

        ConcurrentQueue<IDictionary<string, object>> addQueue = new ConcurrentQueue<IDictionary<string, object>>();
        ConcurrentQueue<IDictionary<string, object>> updateQueue = new ConcurrentQueue<IDictionary<string, object>>();

        ConcurrentDictionary<ITest, int> unqiueIdToTestIdMap = new ConcurrentDictionary<ITest, int>();

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
                    workEvent.WaitOne(); // Wait for work

                    // Get local copies of the queues
                    var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<IDictionary<string, object>>());
                    var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<IDictionary<string, object>>());

                    if (previousErrors)
                        break;

                    // We have to do add's before update because we need the test id from the add to inject into the update
                    await SendTestResults(true, runId.Value, aq.ToArray())
                        .ConfigureAwait(false);
                    await SendTestResults(false, runId.Value, uq.ToArray())
                        .ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"VstsClient.RunLoop: Could not create test run. Message: {e.Message}");
            }
            finally
            {
                try
                {
                    if (runId.HasValue)
                        await FinishTestRun(runId.Value);
                    else
                    {
                        logger.LogError("RunId is not set, cannot complete test run");
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"VstsClient.RunLoop: Could not finish test run. Message: {e.Message}");
                }
                finished.Set();
            }
        }


        public void AddTest(IDictionary<string, object> request, ITest uniqueId)
        {
            request.Add(UNIQUEIDKEY, uniqueId);
            addQueue.Enqueue(request);
            workEvent.Set();
        }

        public void UpdateTest(IDictionary<string, object> request, ITest uniqueId)
        {
            request.Add(UNIQUEIDKEY, uniqueId);
            updateQueue.Enqueue(request);
            workEvent.Set();
        }

        async Task<int> CreateTestRun()
        {
            var requestMessage = new Dictionary<string, object>
            {
                { "name", $"xUnit Runner Test Run on {DateTime.UtcNow.ToString("o")}"},
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
            var url = $"{baseUri}?api-version=1.0";
            string respString = null;
            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(HttpMethod.Post, url)
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
                        logger.LogWarning($"When sending 'POST {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        previousErrors = true;
                    }


                    using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()
                                                                       .ConfigureAwait(false)))
                    {
                        respString = await reader.ReadToEndAsync();
                        //logger.LogMessage($"Rest Run created:\n{respString}");
                        using (var sr = new StringReader(respString))
                        {

                            var resp = JsonDeserializer.Deserialize(sr) as JsonObject;
                            var id = resp.ValueAsInt("id");
                            return id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending 'POST {url}' with body '{bodyString}'\nexception was thrown: {ex.Message}\nresponse string:\n{respString}");
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
            var url = $"{baseUri}/{testRunId}?api-version=1.0";
            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);
                

                var request = new HttpRequestMessage(PatchHttpMethod, url)
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
                        logger.LogWarning($"When sending 'PATCH {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
                        previousErrors = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending 'PATCH {url}' with body '{bodyString}', exception was thrown: {ex.Message}");
                throw;
            }
        }


        async Task SendTestResults(bool isAdd, int runId, ICollection<IDictionary<string, object>> body)
        {
            if (body.Count == 0)
                return;

            // For adds, we need to remove the unique id's and correlate to the responses
            // For update we need to look up the reponses

        //    var originalBody = ToJson(body);

            List<ITest> added = null;
            if (isAdd)
            {
                added = new List<ITest>(body.Count);

                // Add them to the list so we can ref by ordinal on the response
                foreach (var item in body)
                {
                    var uniqueId = (ITest)item[UNIQUEIDKEY];
                    item.Remove(UNIQUEIDKEY);

                    added.Add(uniqueId);
                }
            }
            else
            {
                // The values should be in the map
                foreach (var item in body)
                {
                    var uniqueId = (ITest)item[UNIQUEIDKEY];
                    item.Remove(UNIQUEIDKEY);

                    // lookup and add
                    var testId = unqiueIdToTestIdMap[uniqueId];
                    item.Add("id", testId);
                }
            }

            var method = isAdd ? HttpMethod.Post : PatchHttpMethod;
            var bodyString = ToJson(body);

            var url = $"{baseUri}/{runId}/results?api-version=3.0-preview";

            try
            {
                var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

                var request = new HttpRequestMessage(method, url)
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
                        logger.LogWarning($"When sending '{method} {url}', received status code '{response.StatusCode}'; request body:\n{bodyString}");
                        previousErrors = true;
                    }

                    if (isAdd)
                    {
                        string respString = null;
                        // We need to process the repsonse to extract the Id's and add them to the map 
                        using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()
                                                                           .ConfigureAwait(false)))
                        {
                            respString = await reader.ReadToEndAsync();

                            using (var sr = new StringReader(respString))
                            {
                                var resp = JsonDeserializer.Deserialize(sr) as JsonObject;

                                var testCases = resp.Value("value") as JsonArray;
                                for (var i = 0; i < testCases.Length; ++i)
                                {
                                    var testCase = testCases[i] as JsonObject;
                                    var id = testCase.ValueAsInt("id");

                                    // Match the unique id by ordinal
                                    var uniqueId = added[i];
                                    unqiueIdToTestIdMap[uniqueId] = id;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"When sending '{method} {url}' with body '{bodyString}', exception was thrown: {ex.Message}");
                throw;
            }
        }


        static string ToJson(IEnumerable<IDictionary<string, object>> data)
        {
            var results = string.Join(",", data.Select(x => x.ToJson()));
            return $"[{results}]";
        }

        bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    workEvent.Dispose();
                }


                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
