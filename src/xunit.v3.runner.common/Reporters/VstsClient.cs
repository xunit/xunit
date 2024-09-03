using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

sealed class VstsClient : IDisposable
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
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNullOrEmpty(baseUri);
		Guard.ArgumentNotNullOrEmpty(accessToken);

		this.logger = logger;
		this.baseUri = baseUri;
		this.buildId = buildId;

		client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

		Task.Run(RunLoop);
	}

	public void Dispose()
	{
		// Free up to process any remaining work
		shouldExit = true;
		workEvent.Set();

		finished.Wait();
		finished.Dispose();

		workEvent.Dispose();
		client.Dispose();
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
			logger.LogError("VstsClient.RunLoop: Could not create test run. Message: {0}", e.Message);
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
				logger.LogError("VstsClient.RunLoop: Could not finish test run. Message: {0}", e.Message);
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

	async Task<int> CreateTestRun()
	{
		var buffer = new StringBuilder();

		using (var messageSerializer = new JsonObjectSerializer(buffer))
		{
			messageSerializer.Serialize("name", string.Format(CultureInfo.CurrentCulture, "xUnit Runner Test Run on {0:o}", DateTime.UtcNow));
			using (var buildSerializer = messageSerializer.SerializeObject("build"))
				buildSerializer.Serialize("id", buildId);
			messageSerializer.Serialize("isAutomated", true);
		}

		var bodyString = buffer.ToString();
		var url = baseUri + "?api-version=1.0";
		var responseString = default(string);

		try
		{
			var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

			using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new ByteArrayContent(bodyBytes) };
			request.Content.Headers.ContentType = JsonMediaType;
			request.Headers.Accept.Add(JsonMediaType);

			using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning("When sending 'POST {0}', received status code '{1}'; request body: {2}", url, response.StatusCode, bodyString);
				previousErrors = true;
			}

			responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return !JsonDeserializer.TryDeserialize(responseString, out var root)
				? throw new InvalidOperationException("Response was not JSON")
				: root is not IReadOnlyDictionary<string, object?> rootObject
					? throw new InvalidOperationException("Response was not a JSON object")
					: !rootObject.TryGetValue("id", out var idProp) || idProp is not decimal id || id % 1 != 0
						? throw new InvalidOperationException("Response JSON did not have an integer 'id' property")
						: (int)id;
		}
		catch (Exception ex)
		{
			logger.LogError("When sending 'POST {0}' with body '{1}'\nexception was thrown: {2}\nresponse string:\n{3}", url, bodyString, ex.Message, responseString);
			throw;
		}
	}

	async Task FinishTestRun(int testRunId)
	{
		var buffer = new StringBuilder();

		using (var messageSerializer = new JsonObjectSerializer(buffer))
		{
			messageSerializer.Serialize("completedDate", DateTimeOffset.UtcNow);
			messageSerializer.Serialize("state", "completed");
		}

		var bodyString = buffer.ToString();
		var url = string.Format(CultureInfo.InvariantCulture, "{0}/{1}?api-version=1.0", baseUri, testRunId);

		try
		{
			var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

			using var request = new HttpRequestMessage(PatchHttpMethod, url) { Content = new ByteArrayContent(bodyBytes) };
			request.Content.Headers.ContentType = JsonMediaType;
			request.Headers.Accept.Add(JsonMediaType);

			using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning("When sending 'PATCH {0}', received status code '{1}'; request body: {2}", url, response.StatusCode, bodyString);
				previousErrors = true;
			}
		}
		catch (Exception ex)
		{
			logger.LogError("When sending 'PATCH {0}' with body '{1}', exception was thrown: {2}", url, bodyString, ex.Message);
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

		var url = string.Format(CultureInfo.InvariantCulture, "{0}/{1}/results?api-version=3.0-preview", baseUri, runId);

		try
		{
			var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

			using var request = new HttpRequestMessage(method, url) { Content = new ByteArrayContent(bodyBytes) };
			request.Content.Headers.ContentType = JsonMediaType;
			request.Headers.Accept.Add(JsonMediaType);

			using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning("When sending '{0} {1}', received status code '{2}'; request body:\n{3}", method, url, response.StatusCode, bodyString);
				previousErrors = true;
			}

			if (isAdd)
			{
				var respString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

				if (!JsonDeserializer.TryDeserialize(respString, out var root))
					throw new InvalidOperationException("Response was not JSON");
				if (root is not IReadOnlyDictionary<string, object?> rootObject)
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "JSON response was not in the proper format (expected Object, got {0})", root?.GetType().SafeName() ?? "null"));

				if (!rootObject.TryGetValue("value", out var testCasesValue) || testCasesValue is not object?[] testCases)
					throw new InvalidOperationException("JSON response was missing top-level 'value' array");

				for (var i = 0; i < testCases.Length; ++i)
				{
					var testCase = testCases[i];
					if (testCase is not IReadOnlyDictionary<string, object?> testCaseObject)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "JSON response value element {0} was not in the proper format (expected Object, got {1})", i, testCase?.GetType().SafeName() ?? "null"));

					if (!testCaseObject.TryGetValue("id", out var idProp) || idProp is not decimal id || id % 1 != 0)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "JSON response value element {0} is missing an 'id' property or it wasn't an integer", i));

					// Match the test by ordinal
					var test = added![i];
					testToTestIdMap[test] = (int)id;
				}
			}
		}
		catch (Exception ex)
		{
			logger.LogError("When sending '{0} {1}' with body '{2}', exception was thrown: {3}", method, url, bodyString, ex.Message);
			throw;
		}
	}

	static string ToJson(IEnumerable<IDictionary<string, object?>> data)
	{
		var buffer = new StringBuilder();

		using (var rootSerializer = new JsonArraySerializer(buffer))
			foreach (var dataRow in data)
				using (var objectSerializer = rootSerializer.SerializeObject())
					foreach (var kvp in dataRow)
						// We know from VstsReporterMessageHandler.VstsAddTest() and .VstsUpdateTest() that the only
						// possible values are string, long, or DateTimeOffset, so we serialize only those types.
						if (kvp.Value is long longValue)
							objectSerializer.Serialize(kvp.Key, longValue);
						else if (kvp.Value is DateTimeOffset dtoValue)
							objectSerializer.Serialize(kvp.Key, dtoValue);
						else
							objectSerializer.Serialize(kvp.Key, kvp.Value as string, includeNullValues: true);

		return buffer.ToString();
	}
}
