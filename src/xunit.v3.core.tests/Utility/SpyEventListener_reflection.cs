using System.Diagnostics.Tracing;
using Xunit;

[CollectionDefinition(DisableParallelization = true)]
public class SpyEventListener : EventListener
{
	readonly List<string> events = [];

	public SpyEventListener() =>
		EnableEvents(TestEventSource.Log, EventLevel.LogAlways);

	protected override void OnEventWritten(EventWrittenEventArgs eventData)
	{
		lock (events)
			if (eventData.Payload is null || eventData.PayloadNames is null || eventData.Payload.Count != eventData.PayloadNames.Count)
				events.Add($"[{eventData.EventName}]");
			else
			{
				var payload = new List<string>();
				for (var idx = 0; idx < eventData.Payload.Count; ++idx)
					payload.Add($"{eventData.PayloadNames[idx]} = \"{eventData.Payload[idx]}\"");

				events.Add($"[{eventData.EventName}] {string.Join(", ", payload)}");
			}
	}

	public async ValueTask<string[]> WaitForEventCount(int count)
	{
		var start = DateTimeOffset.UtcNow;
		var max = TimeSpan.FromSeconds(5);

		while (true)
		{
			if (DateTimeOffset.UtcNow - start > max)
				throw new InvalidOperationException($"Never arrived at the final event message count (got {events.Count})");

			lock (events)
				if (events.Count == count)
					return events.ToArray();

			await Task.Yield();
		}
	}
}
