using System.Collections.Generic;
using Xunit.Runner.Common;

public class SpyTestMessageSink : TestMessageSink
{
	public List<string> Calls = new List<string>();

	public SpyTestMessageSink()
	{
		Diagnostics.DiagnosticMessageEvent += args => Calls.Add("_DiagnosticMessage");
		Diagnostics.ErrorMessageEvent += args => Calls.Add("_ErrorMessage");

		Discovery.DiscoveryCompleteEvent += args => Calls.Add("_DiscoveryComplete");
		Discovery.DiscoveryStartingEvent += args => Calls.Add("_DiscoveryStarting");
		Discovery.TestCaseDiscoveredEvent += args => Calls.Add("_TestCaseDiscovered");

		Execution.AfterTestFinishedEvent += args => Calls.Add("_AfterTestFinished");
		Execution.AfterTestStartingEvent += args => Calls.Add("_AfterTestStarting");
		Execution.BeforeTestFinishedEvent += args => Calls.Add("_BeforeTestFinished");
		Execution.BeforeTestStartingEvent += args => Calls.Add("_BeforeTestStarting");
		Execution.TestAssemblyCleanupFailureEvent += args => Calls.Add("_TestAssemblyCleanupFailure");
		Execution.TestAssemblyFinishedEvent += args => Calls.Add("_TestAssemblyFinished");
		Execution.TestAssemblyStartingEvent += args => Calls.Add("_TestAssemblyStarting");
		Execution.TestCaseCleanupFailureEvent += args => Calls.Add("_TestCaseCleanupFailure");
		Execution.TestCaseFinishedEvent += args => Calls.Add("_TestCaseFinished");
		Execution.TestCaseStartingEvent += args => Calls.Add("_TestCaseStarting");
		Execution.TestClassCleanupFailureEvent += args => Calls.Add("_TestClassCleanupFailure");
		Execution.TestClassConstructionFinishedEvent += args => Calls.Add("_TestClassConstructionFinished");
		Execution.TestClassConstructionStartingEvent += args => Calls.Add("_TestClassConstructionStarting");
		Execution.TestClassDisposeFinishedEvent += args => Calls.Add("_TestClassDisposeFinished");
		Execution.TestClassDisposeStartingEvent += args => Calls.Add("_TestClassDisposeStarting");
		Execution.TestClassFinishedEvent += args => Calls.Add("_TestClassFinished");
		Execution.TestClassStartingEvent += args => Calls.Add("_TestClassStarting");
		Execution.TestCleanupFailureEvent += args => Calls.Add("_TestCleanupFailure");
		Execution.TestCollectionCleanupFailureEvent += args => Calls.Add("_TestCollectionCleanupFailure");
		Execution.TestCollectionFinishedEvent += args => Calls.Add("_TestCollectionFinished");
		Execution.TestCollectionStartingEvent += args => Calls.Add("_TestCollectionStarting");
		Execution.TestFailedEvent += args => Calls.Add("_TestFailed");
		Execution.TestFinishedEvent += args => Calls.Add("_TestFinished");
		Execution.TestMethodCleanupFailureEvent += args => Calls.Add("_TestMethodCleanupFailure");
		Execution.TestMethodFinishedEvent += args => Calls.Add("_TestMethodFinished");
		Execution.TestMethodStartingEvent += args => Calls.Add("_TestMethodStarting");
		Execution.TestOutputEvent += args => Calls.Add("_TestOutput");
		Execution.TestPassedEvent += args => Calls.Add("_TestPassed");
		Execution.TestSkippedEvent += args => Calls.Add("_TestSkipped");
		Execution.TestStartingEvent += args => Calls.Add("_TestStarting");

		Runner.TestAssemblyDiscoveryFinishedEvent += args => Calls.Add("TestAssemblyDiscoveryFinished");
		Runner.TestAssemblyDiscoveryStartingEvent += args => Calls.Add("TestAssemblyDiscoveryStarting");
		Runner.TestAssemblyExecutionFinishedEvent += args => Calls.Add("TestAssemblyExecutionFinished");
		Runner.TestAssemblyExecutionStartingEvent += args => Calls.Add("TestAssemblyExecutionStarting");
		Runner.TestExecutionSummariesEvent += args => Calls.Add("TestExecutionSummaries");
	}
}
