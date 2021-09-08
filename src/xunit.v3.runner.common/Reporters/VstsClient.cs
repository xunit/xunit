using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.Runner.Common
{
	class VstsClient
	{
		static readonly MediaTypeWithQualityHeaderValue JsonMediaType = new("application/json");
		static readonly HttpMethod PatchHttpMethod = new("PATCH");
		const string UNIQUEIDKEY = "UNIQUEIDKEY";

		ConcurrentQueue<IDictionary<string, object?>> addQueue = new();
		readonly string baseUri;
		readonly int buildId;
		readonly HttpClient client;
		readonly ManualResetEventSlim finished = new(initialState: false);
		readonly IRunnerLogger logger;
		volatile bool previousErrors;
		volatile bool shouldExit;
		readonly ConcurrentDictionary<string, int> testToTestIdMap = new();
		ConcurrentQueue<IDictionary<string, object?>> updateQueue = new();
		readonly AutoResetEvent workEvent = new(initialState: false);

		public VstsClient(
			IRunnerLogger logger,
			string baseUri,
			string accessToken,
			int buildId)
		{
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNullOrEmpty(nameof(baseUri), baseUri);
			Guard.ArgumentNotNullOrEmpty(nameof(accessToken), accessToken);

			this.logger = logger;
			this.baseUri = baseUri;
			this.buildId = buildId;

			client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			Task.Run(RunLoop);
		}

		public void Dispose(CancellationToken cancellationToken)
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
					var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<IDictionary<string, object?>>());
					var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<IDictionary<string, object?>>());

					if (previousErrors)
						break;

					// We have to do adds before update because we need the test ID from the add to inject into the update
					await SendTestResults(true, runId.Value, aq.ToArray()).ConfigureAwait(false);
					await SendTestResults(false, runId.Value, uq.ToArray()).ConfigureAwait(false);
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
						logger.LogError("RunId is not set, cannot complete test run");
				}
				catch (Exception e)
				{
					logger.LogError($"VstsClient.RunLoop: Could not finish test run. Message: {e.Message}");
				}

				finished.Set();
			}
		}

		public void AddTest(
			IDictionary<string, object?> request,
			string testUniqueID)
		{
			request.Add(UNIQUEIDKEY, testUniqueID);
			addQueue.Enqueue(request);
			workEvent.Set();
		}

		public void UpdateTest(
			IDictionary<string, object?> request,
			string testUniqueID)
		{
			request.Add(UNIQUEIDKEY, testUniqueID);
			updateQueue.Enqueue(request);
			workEvent.Set();
		}

		async Task<int?> CreateTestRun()
		{
			var requestMessage = new Dictionary<string, object?>
			{
				{ "name", $"xUnit Runner Test Run on {DateTime.UtcNow:o}"},
				{ "build", new Dictionary<string, object?> { { "id", buildId } } },
				{ "isAutomated", true }
			};

			var bodyString = JsonSerializer.Serialize(requestMessage);
			var url = $"{baseUri}?api-version=1.0";
			var responseString = default(string);
			try
			{
				var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

				var request = new HttpRequestMessage(HttpMethod.Post, url)
				{
					Content = new ByteArrayContent(bodyBytes)
				};
				request.Content.Headers.ContentType = JsonMediaType;
				request.Headers.Accept.Add(JsonMediaType);

				using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
				var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogWarning($"When sending 'POST {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
					previousErrors = true;
				}

				responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var responseJson = JsonSerializer.Deserialize<JsonElement>(responseString);
				if (responseJson.ValueKind != JsonValueKind.Object)
					throw new InvalidOperationException($"Response was not a JSON object");

				if (!responseJson.TryGetProperty("id", out var idProp) || !(idProp.TryGetInt32(out var id)))
					throw new InvalidOperationException($"Response JSON did not have an integer 'id' property");

				return id;
			}
			catch (Exception ex)
			{
				logger.LogError($"When sending 'POST {url}' with body '{bodyString}'\nexception was thrown: {ex.Message}\nresponse string:\n{responseString}");
				throw;
			}
		}

		async Task FinishTestRun(int testRunId)
		{
			var requestMessage = new Dictionary<string, object?>
			{
				{ "completedDate", DateTime.UtcNow },
				{ "state", "Completed" }
			};

			var bodyString = JsonSerializer.Serialize(requestMessage);
			var url = $"{baseUri}/{testRunId}?api-version=1.0";
			try
			{
				var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

				var request = new HttpRequestMessage(PatchHttpMethod, url)
				{
					Content = new ByteArrayContent(bodyBytes)
				};
				request.Content.Headers.ContentType = JsonMediaType;
				request.Headers.Accept.Add(JsonMediaType);

				using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
				var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogWarning($"When sending 'PATCH {url}', received status code '{response.StatusCode}'; request body: {bodyString}");
					previousErrors = true;
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"When sending 'PATCH {url}' with body '{bodyString}', exception was thrown: {ex.Message}");
				throw;
			}
		}

		async Task SendTestResults(
			bool isAdd,
			int runId,
			ICollection<IDictionary<string, object?>> body)
		{
			if (body.Count == 0)
				return;

			// For adds, we need to remove the unique IDs and correlate to the responses
			// For update we need to look up the responses
			List<string>? added = null;
			if (isAdd)
			{
				added = new List<string>(body.Count);

				// Add them to the list so we can ref by ordinal on the response
				foreach (var item in body)
				{
					var test = (string?)item[UNIQUEIDKEY];
					Guard.NotNull("Pulled null test unique ID from work queue", test);

					item.Remove(UNIQUEIDKEY);
					added.Add(test);
				}
			}
			else
			{
				// The values should be in the map
				foreach (var item in body)
				{
					var test = (string?)item[UNIQUEIDKEY];
					Guard.NotNull("Pulled null test unique ID from work queue", test);

					item.Remove(UNIQUEIDKEY);

					// lookup and add
					var testId = testToTestIdMap[test];
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
				request.Content.Headers.ContentType = JsonMediaType;
				request.Headers.Accept.Add(JsonMediaType);

				using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
				var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
				{
					logger.LogWarning($"When sending '{method} {url}', received status code '{response.StatusCode}'; request body:\n{bodyString}");
					previousErrors = true;
				}

				if (isAdd)
				{
					var respString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

					var responseJson = JsonSerializer.Deserialize<JsonElement>(respString);
					if (responseJson.ValueKind != JsonValueKind.Object)
						throw new InvalidOperationException($"JSON response was not in the proper format (expected Object, got {responseJson.ValueKind})");

					if (!responseJson.TryGetProperty("value", out var testCases) || testCases.ValueKind != JsonValueKind.Array)
						throw new InvalidOperationException("JSON response was missing top-level 'value' array");

					for (var i = 0; i < testCases.GetArrayLength(); ++i)
					{
						var testCase = testCases[i];
						if (testCase.ValueKind != JsonValueKind.Object)
							throw new InvalidOperationException($"JSON response value element {i} was not in the proper format (expected Object, got {testCase.ValueKind})");

						if (!testCase.TryGetProperty("id", out var idProp) || !idProp.TryGetInt32(out var id))
							throw new InvalidOperationException($"JSON response value element {i} is missing an 'id' property or it wasn't an integer");

						// Match the test by ordinal
						var test = added![i];
						testToTestIdMap[test] = id;
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"When sending '{method} {url}' with body '{bodyString}', exception was thrown: {ex.Message}");
				throw;
			}
		}

		static string ToJson(IEnumerable<IDictionary<string, object?>> data)
		{
			var results = string.Join(",", data.Select(x => JsonSerializer.Serialize(x)));
			return $"[{results}]";
		}
	}
}
