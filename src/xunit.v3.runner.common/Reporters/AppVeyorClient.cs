using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

sealed class AppVeyorClient : IDisposable
{
	ConcurrentQueue<IDictionary<string, object?>> addQueue = new();
	readonly string baseUri;
	readonly HttpClient client = new();
	readonly ManualResetEventSlim finished = new(initialState: false);
	readonly MediaTypeWithQualityHeaderValue jsonMediaType = new("application/json");
	readonly IRunnerLogger logger;
	volatile bool previousErrors;
	volatile bool shouldExit;
	ConcurrentQueue<IDictionary<string, object?>> updateQueue = new();
	readonly ManualResetEventSlim workEvent = new(initialState: false);

	public AppVeyorClient(
		IRunnerLogger logger,
		string baseUri)
	{
		this.logger = logger;
		this.baseUri = string.Format(CultureInfo.InvariantCulture, "{0}/api/tests/batch", baseUri);

		Task.Run(RunLoop);
	}

	public void AddTest(IDictionary<string, object?> request)
	{
		addQueue.Enqueue(request);
		workEvent.Set();
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
		try
		{
			while (!shouldExit || !addQueue.IsEmpty || !updateQueue.IsEmpty)
			{
				workEvent.Wait();   // Wait for work
				workEvent.Reset();  // Reset first to ensure any subsequent modification sets

				// Get local copies of the queues
				var aq = Interlocked.Exchange(ref addQueue, new ConcurrentQueue<IDictionary<string, object?>>());
				var uq = Interlocked.Exchange(ref updateQueue, new ConcurrentQueue<IDictionary<string, object?>>());

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

	async Task SendRequest(
		HttpMethod method,
		ICollection<IDictionary<string, object?>> body)
	{
		if (body.Count == 0)
			return;

		var bodyString = ToJson(body);

		try
		{
			var bodyBytes = Encoding.UTF8.GetBytes(bodyString);

			using var request = new HttpRequestMessage(method, baseUri) { Content = new ByteArrayContent(bodyBytes) };
			request.Content.Headers.ContentType = jsonMediaType;
			request.Headers.Accept.Add(jsonMediaType);

			using var tcs = new CancellationTokenSource(TimeSpan.FromSeconds(30));
			var response = await client.SendAsync(request, tcs.Token).ConfigureAwait(false);
			if (!response.IsSuccessStatusCode)
			{
				logger.LogWarning(
					string.Format(
						CultureInfo.CurrentCulture,
						"When sending '{0} {1}', received status code '{2}'; request body: {3}",
						method,
						baseUri,
						response.StatusCode,
						bodyString
					)
				);

				previousErrors = true;
			}
		}
		catch (Exception ex)
		{
			logger.LogError(
				string.Format(
					CultureInfo.CurrentCulture,
					"When sending '{0} {1}' with body '{2}', exception was thrown: {3}",
					method,
					baseUri,
					bodyString,
					ex.Message
				)
			);

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
						// We know from AppVeyorReporterMessageHandler.GetRequestMessage() that the only possible values are
						// string or long, so we serialize only those types.
						if (kvp.Value is long longValue)
							objectSerializer.Serialize(kvp.Key, longValue);
						else
							objectSerializer.Serialize(kvp.Key, kvp.Value as string, includeNullValues: true);

		return buffer.ToString();
	}

	public void UpdateTest(IDictionary<string, object?> request)
	{
		updateQueue.Enqueue(request);
		workEvent.Set();
	}
}
