using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Runner.Reporters
{
    public class AppVeyorClient
    {
        readonly IRunnerLogger logger;
        readonly string baseUri;

        public AppVeyorClient(IRunnerLogger logger, string baseUri)
        {
            this.logger = logger;
            this.baseUri = string.Format(CultureInfo.InvariantCulture, "{0}/api/tests/batch", baseUri);

            workTask = Task.Run(RunLoop);
        }

        readonly HttpClient client = new HttpClient();
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
            try
            {
                while (!shouldExit || !addQueue.IsEmpty || !updateQueue.IsEmpty)
                {
                    workEvent.Wait();   // Wait for work
                    workEvent.Reset();  // Reset first to ensure any subsequent modification sets

                    // Get local copies of the queues
                    var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<IDictionary<string, object>>());
                    var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<IDictionary<string, object>>());

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

        async Task SendRequest(HttpMethod method, ICollection<IDictionary<string, object>> body)
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
                        logger.LogWarning("When sending '{0} {1}', received status code '{2}'; request body: {3}", method, baseUri, response.StatusCode, bodyString);
                        previousErrors = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("When sending '{0} {1}' with body '{2}', exception was thrown: {3}", method, baseUri, bodyString, ex.Message);
                throw;
            }
        }


        static string ToJson(IEnumerable<IDictionary<string, object>> data)
        {
            var results = string.Join(",", data.Select(x => x.ToJson()));
            return string.Format(CultureInfo.InvariantCulture, "[{0}]", results);
        }
    }
}
