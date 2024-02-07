using System.Diagnostics.Tracing;

namespace Xunit.v3;

[EventSource(Name = SourceName, Guid = "ae399e80-45fc-4219-aacc-b73a458ad7e1")]
internal sealed class TestEventSource : EventSource
{
	internal const string SourceName = "xUnit.TestEventSource";

	internal const int TestStartEventId = 1;
	internal const int TestStopEventId = 2;

	internal class Tasks
	{
		internal const EventTask Test = (EventTask)1;
	}

	internal static readonly TestEventSource Log = new TestEventSource();

	private TestEventSource() { }

	[Event(TestStartEventId, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Start)]
	internal void TestMethodStart(string testDisplayName) => WriteEvent(TestStartEventId, testDisplayName);

	[Event(TestStopEventId, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Stop)]
	internal void TestMethodStop(string testDisplayName) => WriteEvent(TestStopEventId, testDisplayName);
}
