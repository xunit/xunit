using System.Diagnostics.Tracing;

namespace Xunit.v3;

[EventSource(Name = sourceName, Guid = "ae399e80-45fc-4219-aacc-b73a458ad7e1")]
internal sealed class TestEventSource : EventSource
{
	const string sourceName = "xUnit.TestEventSource";

	internal static readonly TestEventSource Log = new();

	TestEventSource() { }

	// Start/stop for test

	static partial class Tasks { internal const EventTask Test = (EventTask)1; }
	static partial class Events { internal const int TestStart = 1; internal const int TestStop = 2; }

	[Event(Events.TestStart, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Start)]
	internal void TestStart(string testName) =>
		WriteEvent(Events.TestStart, testName);

	[Event(Events.TestStop, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Stop)]
	internal void TestStop(string testName) =>
		WriteEvent(Events.TestStop, testName);

	// Start/stop for test case

	static partial class Tasks { internal const EventTask TestCase = (EventTask)2; }
	static partial class Events { internal const int TestCaseStart = 11; internal const int TestCaseStop = 12; }

	[Event(Events.TestCaseStart, Level = EventLevel.Informational, Task = Tasks.TestCase, Opcode = EventOpcode.Start)]
	internal void TestCaseStart(string testCaseName) =>
		WriteEvent(Events.TestCaseStart, testCaseName);

	[Event(Events.TestCaseStop, Level = EventLevel.Informational, Task = Tasks.TestCase, Opcode = EventOpcode.Stop)]
	internal void TestCaseStop(string testCaseName) =>
		WriteEvent(Events.TestCaseStop, testCaseName);

	// Start/stop for test method

	static partial class Tasks { internal const EventTask TestMethod = (EventTask)3; }
	static partial class Events { internal const int TestMethodStart = 21; internal const int TestMethodStop = 22; }

	[Event(Events.TestMethodStart, Level = EventLevel.Informational, Task = Tasks.TestMethod, Opcode = EventOpcode.Start)]
	internal void TestMethodStart(string testMethodName) =>
		WriteEvent(Events.TestMethodStart, testMethodName);

	[Event(Events.TestMethodStop, Level = EventLevel.Informational, Task = Tasks.TestMethod, Opcode = EventOpcode.Stop)]
	internal void TestMethodStop(string testMethodName) =>
		WriteEvent(Events.TestMethodStop, testMethodName);

	// Start/stop for test class

	static partial class Tasks { internal const EventTask TestClass = (EventTask)4; }
	static partial class Events { internal const int TestClassStart = 31; internal const int TestClassStop = 32; }

	[Event(Events.TestClassStart, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Start)]
	internal void TestClassStart(string testClassName) =>
		WriteEvent(Events.TestClassStart, testClassName);

	[Event(Events.TestClassStop, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Stop)]
	internal void TestClassStop(string testClassName) =>
		WriteEvent(Events.TestClassStop, testClassName);

	// Start/stop for test collection

	static partial class Tasks { internal const EventTask TestCollection = (EventTask)5; }
	static partial class Events { internal const int TestCollectionStart = 41; internal const int TestCollectionStop = 42; }

	[Event(Events.TestCollectionStart, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Start)]
	internal void TestCollectionStart(string testCollectionName) =>
		WriteEvent(Events.TestCollectionStart, testCollectionName);

	[Event(Events.TestCollectionStop, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Stop)]
	internal void TestCollectionStop(string testCollectionName) =>
		WriteEvent(Events.TestCollectionStop, testCollectionName);

	// Start/stop for test assembly

	static partial class Tasks { internal const EventTask TestAssembly = (EventTask)6; }
	static partial class Events { internal const int TestAssemblyStart = 51; internal const int TestAssemblyStop = 52; }

	[Event(Events.TestAssemblyStart, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Start)]
	internal void TestAssemblyStart(
		string assemblyPath,
		string configFileName) =>
			WriteEvent(Events.TestAssemblyStart, assemblyPath, configFileName);

	[Event(Events.TestAssemblyStop, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Stop)]
	internal void TestAssemblyStop(
		string assemblyPath,
		string configFileName) =>
			WriteEvent(Events.TestAssemblyStop, assemblyPath, configFileName);
}
