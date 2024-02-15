using System.Diagnostics.Tracing;

namespace Xunit.v3;

[EventSource(Name = sourceName, Guid = "ae399e80-45fc-4219-aacc-b73a458ad7e1")]
internal sealed class TestEventSource : EventSource
{
	const string sourceName = "xUnit.TestEventSource";

	internal static readonly TestEventSource Log = new();

	TestEventSource() { }

	[Event(Events.TestAssemblyStart, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Start)]
	internal void TestAssemblyStart(string assemblyPath, string configFileName) => WriteEvent(Events.TestAssemblyStart, assemblyPath, configFileName);

	[Event(Events.TestAssemblyStop, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Stop)]
	internal void TestAssemblyStop(string assemblyPath, string configFileName) => WriteEvent(Events.TestAssemblyStop, assemblyPath, configFileName);

	[Event(Events.TestClassStart, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Start)]
	internal void TestClassStart(string testClassName) => WriteEvent(Events.TestClassStart, testClassName);

	[Event(Events.TestClassStop, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Stop)]
	internal void TestClassStop(string testClassName) => WriteEvent(Events.TestClassStop, testClassName);

	[Event(Events.TestCollectionStart, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Start)]
	internal void TestCollectionStart(string testCollectionName) => WriteEvent(Events.TestCollectionStart, testCollectionName);

	[Event(Events.TestCollectionStop, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Stop)]
	internal void TestCollectionStop(string testCollectionName) => WriteEvent(Events.TestCollectionStop, testCollectionName);

	[Event(Events.TestStart, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Start)]
	internal void TestStart(string testName) => WriteEvent(Events.TestStart, testName);

	[Event(Events.TestStop, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Stop)]
	internal void TestStop(string testName) => WriteEvent(Events.TestStop, testName);

	class Events
	{
		internal const int TestAssemblyStart = 101;
		internal const int TestAssemblyStop = 102;

		internal const int TestClassStart = 301;
		internal const int TestClassStop = 302;

		internal const int TestCollectionStart = 201;
		internal const int TestCollectionStop = 202;

		internal const int TestStart = 401;
		internal const int TestStop = 402;
	}

	class Tasks
	{
		internal const EventTask TestAssembly = (EventTask)1;
		internal const EventTask TestClass = (EventTask)3;
		internal const EventTask TestCollection = (EventTask)2;
		internal const EventTask Test = (EventTask)4;
	}
}
