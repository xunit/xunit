using System.Collections.Generic;
using Xunit.Runner.Common;

public class SpyTestMessageSink : TestMessageSink
{
	public List<string> Calls = [];

	public SpyTestMessageSink()
	{
		Diagnostics.DiagnosticMessageEvent += args => Calls.Add("DiagnosticMessage");
		Diagnostics.ErrorMessageEvent += args => Calls.Add("ErrorMessage");

		Discovery.DiscoveryCompleteEvent += args => Calls.Add("DiscoveryComplete");
		Discovery.DiscoveryStartingEvent += args => Calls.Add("DiscoveryStarting");
		Discovery.TestCaseDiscoveredEvent += args => Calls.Add("TestCaseDiscovered");

		Execution.AfterTestFinishedEvent += args => Calls.Add("AfterTestFinished");
		Execution.AfterTestStartingEvent += args => Calls.Add("AfterTestStarting");
		Execution.BeforeTestFinishedEvent += args => Calls.Add("BeforeTestFinished");
		Execution.BeforeTestStartingEvent += args => Calls.Add("BeforeTestStarting");
		Execution.TestAssemblyCleanupFailureEvent += args => Calls.Add("TestAssemblyCleanupFailure");
		Execution.TestAssemblyFinishedEvent += args => Calls.Add("TestAssemblyFinished");
		Execution.TestAssemblyStartingEvent += args => Calls.Add("TestAssemblyStarting");
		Execution.TestCaseCleanupFailureEvent += args => Calls.Add("TestCaseCleanupFailure");
		Execution.TestCaseFinishedEvent += args => Calls.Add("TestCaseFinished");
		Execution.TestCaseStartingEvent += args => Calls.Add("TestCaseStarting");
		Execution.TestClassCleanupFailureEvent += args => Calls.Add("TestClassCleanupFailure");
		Execution.TestClassConstructionFinishedEvent += args => Calls.Add("TestClassConstructionFinished");
		Execution.TestClassConstructionStartingEvent += args => Calls.Add("TestClassConstructionStarting");
		Execution.TestClassDisposeFinishedEvent += args => Calls.Add("TestClassDisposeFinished");
		Execution.TestClassDisposeStartingEvent += args => Calls.Add("TestClassDisposeStarting");
		Execution.TestClassFinishedEvent += args => Calls.Add("TestClassFinished");
		Execution.TestClassStartingEvent += args => Calls.Add("TestClassStarting");
		Execution.TestCleanupFailureEvent += args => Calls.Add("TestCleanupFailure");
		Execution.TestCollectionCleanupFailureEvent += args => Calls.Add("TestCollectionCleanupFailure");
		Execution.TestCollectionFinishedEvent += args => Calls.Add("TestCollectionFinished");
		Execution.TestCollectionStartingEvent += args => Calls.Add("TestCollectionStarting");
		Execution.TestFailedEvent += args => Calls.Add("TestFailed");
		Execution.TestFinishedEvent += args => Calls.Add("TestFinished");
		Execution.TestMethodCleanupFailureEvent += args => Calls.Add("TestMethodCleanupFailure");
		Execution.TestMethodFinishedEvent += args => Calls.Add("TestMethodFinished");
		Execution.TestMethodStartingEvent += args => Calls.Add("TestMethodStarting");
		Execution.TestNotRunEvent += args => Calls.Add("TestNotRun");
		Execution.TestOutputEvent += args => Calls.Add("TestOutput");
		Execution.TestPassedEvent += args => Calls.Add("TestPassed");
		Execution.TestSkippedEvent += args => Calls.Add("TestSkipped");
		Execution.TestStartingEvent += args => Calls.Add("TestStarting");

		Runner.TestAssemblyDiscoveryFinishedEvent += args => Calls.Add("TestAssemblyDiscoveryFinished");
		Runner.TestAssemblyDiscoveryStartingEvent += args => Calls.Add("TestAssemblyDiscoveryStarting");
		Runner.TestAssemblyExecutionFinishedEvent += args => Calls.Add("TestAssemblyExecutionFinished");
		Runner.TestAssemblyExecutionStartingEvent += args => Calls.Add("TestAssemblyExecutionStarting");
		Runner.TestExecutionSummariesEvent += args => Calls.Add("TestExecutionSummaries");
	}
}
