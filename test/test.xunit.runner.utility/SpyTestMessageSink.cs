using System.Collections.Generic;
using Xunit;

public class SpyTestMessageSink : TestMessageSink
{
    public List<string> Calls = new List<string>();

    public SpyTestMessageSink()
    {
        AfterTestFinishedEvent += args => Calls.Add("IAfterTestFinished");
        AfterTestStartingEvent += args => Calls.Add("IAfterTestStarting");
        BeforeTestFinishedEvent += args => Calls.Add("IBeforeTestFinished");
        BeforeTestStartingEvent += args => Calls.Add("IBeforeTestStarting");
        DiagnosticMessageEvent += args => Calls.Add("IDiagnosticMessage");
        DiscoveryCompleteMessageEvent += args => Calls.Add("IDiscoveryCompleteMessage");
        ErrorMessageEvent += args => Calls.Add("IErrorMessage");
        TestAssemblyCleanupFailureEvent += args => Calls.Add("ITestAssemblyCleanupFailure");
        TestAssemblyDiscoveryFinishedEvent += args => Calls.Add("ITestAssemblyDiscoveryFinished");
        TestAssemblyDiscoveryStartingEvent += args => Calls.Add("ITestAssemblyDiscoveryStarting");
        TestAssemblyExecutionFinishedEvent += args => Calls.Add("ITestAssemblyExecutionFinished");
        TestAssemblyExecutionStartingEvent += args => Calls.Add("ITestAssemblyExecutionStarting");
        TestAssemblyFinishedEvent += args => Calls.Add("ITestAssemblyFinished");
        TestAssemblyStartingEvent += args => Calls.Add("ITestAssemblyStarting");
        TestCaseCleanupFailureEvent += args => Calls.Add("ITestCaseCleanupFailure");
        TestCaseDiscoveryMessageEvent += args => Calls.Add("ITestCaseDiscoveryMessage");
        TestCaseFinishedEvent += args => Calls.Add("ITestCaseFinished");
        TestCaseStartingEvent += args => Calls.Add("ITestCaseStarting");
        TestClassCleanupFailureEvent += args => Calls.Add("ITestClassCleanupFailure");
        TestClassConstructionFinishedEvent += args => Calls.Add("ITestClassConstructionFinished");
        TestClassConstructionStartingEvent += args => Calls.Add("ITestClassConstructionStarting");
        TestClassDisposeFinishedEvent += args => Calls.Add("ITestClassDisposeFinished");
        TestClassDisposeStartingEvent += args => Calls.Add("ITestClassDisposeStarting");
        TestClassFinishedEvent += args => Calls.Add("ITestClassFinished");
        TestClassStartingEvent += args => Calls.Add("ITestClassStarting");
        TestCleanupFailureEvent += args => Calls.Add("ITestCleanupFailure");
        TestCollectionCleanupFailureEvent += args => Calls.Add("ITestCollectionCleanupFailure");
        TestCollectionFinishedEvent += args => Calls.Add("ITestCollectionFinished");
        TestCollectionStartingEvent += args => Calls.Add("ITestCollectionStarting");
        TestExecutionSummaryEvent += args => Calls.Add("ITestExecutionSummary");
        TestFailedEvent += args => Calls.Add("ITestFailed");
        TestFinishedEvent += args => Calls.Add("ITestFinished");
        TestMethodCleanupFailureEvent += args => Calls.Add("ITestMethodCleanupFailure");
        TestMethodFinishedEvent += args => Calls.Add("ITestMethodFinished");
        TestMethodStartingEvent += args => Calls.Add("ITestMethodStarting");
        TestOutputEvent += args => Calls.Add("ITestOutput");
        TestPassedEvent += args => Calls.Add("ITestPassed");
        TestSkippedEvent += args => Calls.Add("ITestSkipped");
        TestStartingEvent += args => Calls.Add("ITestStarting");
    }
}
