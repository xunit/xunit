using System.Diagnostics.Tracing;

namespace Xunit.v3;

[EventSource(Name = sourceName, Guid = "ae399e80-45fc-4219-aacc-b73a458ad7e1")]
internal sealed class TestEventSource : EventSource
{
	const string sourceName = "xUnit.TestEventSource";

	internal static readonly TestEventSource Log = new();

	TestEventSource() { }

	[Event(Events.TestAssemblyStart, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Start)]
	internal void TestAssemblyStart(_ITestAssembly testAssembly) => WriteEvent(Events.TestAssemblyStart, testAssembly.Assembly.AssemblyPath, testAssembly.ConfigFileName);

	[Event(Events.TestAssemblyStop, Level = EventLevel.Informational, Task = Tasks.TestAssembly, Opcode = EventOpcode.Stop)]
	internal void TestAssemblyStop(_ITestAssembly testAssembly) => WriteEvent(Events.TestAssemblyStop, testAssembly.Assembly.AssemblyPath, testAssembly.ConfigFileName);

	[Event(Events.TestClassStart, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Start)]
	internal void TestClassStart(_ITestClass testClass) => WriteEvent(Events.TestClassStart, testClass.Class.Name);

	[Event(Events.TestClassStop, Level = EventLevel.Informational, Task = Tasks.TestClass, Opcode = EventOpcode.Stop)]
	internal void TestClassStop(_ITestClass testClass) => WriteEvent(Events.TestClassStop, testClass.Class.Name);

	[Event(Events.TestCollectionStart, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Start)]
	internal void TestCollectionStart(_ITestCollection collection) => WriteEvent(Events.TestCollectionStart, collection.DisplayName);

	[Event(Events.TestCollectionStop, Level = EventLevel.Informational, Task = Tasks.TestCollection, Opcode = EventOpcode.Stop)]
	internal void TestCollectionStop(_ITestCollection collection) => WriteEvent(Events.TestCollectionStop, collection.DisplayName);

	[Event(Events.TestStart, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Start)]
	internal void TestStart(_ITest test) => WriteEvent(Events.TestStart, test.TestDisplayName);

	[Event(Events.TestStop, Level = EventLevel.Informational, Task = Tasks.Test, Opcode = EventOpcode.Stop)]
	internal void TestStop(_ITest test) => WriteEvent(Events.TestStop, test.TestDisplayName);

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
