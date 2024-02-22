using System.Diagnostics.Tracing;

namespace Xunit.Sdk
{
    [EventSource(Name = sourceName, Guid = "ae399e80-45fc-4219-aacc-b73a458ad7e1")]
    internal sealed class TestEventSource : EventSource
    {
        const string sourceName = "xUnit.TestEventSource";

        internal static readonly TestEventSource Log = new();

        TestEventSource() { }

        [Event(Events.TestStart, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Start)]
        internal void TestStart(string testName) => WriteEvent(Events.TestStart, testName);

        [Event(Events.TestStop, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Stop)]
        internal void TestStop(string testName) => WriteEvent(Events.TestStop, testName);

        class Events
        {
            internal const int TestStart = 1;
            internal const int TestStop = 2;
        }

        class Tasks
        {
            internal const EventTask Test = (EventTask)1;
        }
    }
}
