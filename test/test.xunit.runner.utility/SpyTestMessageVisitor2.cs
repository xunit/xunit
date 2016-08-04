using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

public class SpyTestMessageVisitor2 : TestMessageVisitor2
{
    public List<string> Calls = new List<string>();

    public SpyTestMessageVisitor2()
    {
        AfterTestFinishedEvent += Visit;
        AfterTestStartingEvent += Visit;
        BeforeTestFinishedEvent += Visit;
        BeforeTestStartingEvent += Visit;
        DiagnosticMessageEvent += Visit;
        DiscoveryCompleteMessageEvent += Visit;
        ErrorMessageEvent += Visit;
        TestAssemblyCleanupFailureEvent += Visit;
        TestAssemblyDiscoveryFinishedEvent += Visit;
        TestAssemblyDiscoveryStartingEvent += Visit;
        TestAssemblyExecutionFinishedEvent += Visit;
        TestAssemblyExecutionStartingEvent += Visit;
        TestAssemblyFinishedEvent += Visit;
        TestAssemblyStartingEvent += Visit;
        TestCaseCleanupFailureEvent += Visit;
        TestCaseDiscoveryMessageEvent += Visit;
        TestCaseFinishedEvent += Visit;
        TestCaseStartingEvent += Visit;
        TestClassCleanupFailureEvent += Visit;
        TestClassConstructionFinishedEvent += Visit;
        TestClassConstructionStartingEvent += Visit;
        TestClassDisposeFinishedEvent += Visit;
        TestClassDisposeStartingEvent += Visit;
        TestClassFinishedEvent += Visit;
        TestClassStartingEvent += Visit;
        TestCleanupFailureEvent += Visit;
        TestCollectionCleanupFailureEvent += Visit;
        TestCollectionFinishedEvent += Visit;
        TestCollectionStartingEvent += Visit;
        TestExecutionSummaryEvent += Visit;
        TestFailedEvent += Visit;
        TestFinishedEvent += Visit;
        TestMethodCleanupFailureEvent += Visit;
        TestMethodFinishedEvent += Visit;
        TestMethodStartingEvent += Visit;
        TestOutputEvent += Visit;
        TestPassedEvent += Visit;
        TestSkippedEvent += Visit;
        TestStartingEvent += Visit;
    }

    void Visit(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
    {
        Calls.Add("ITestCollectionCleanupFailure");
    }

    void Visit(MessageHandlerArgs<ITestOutput> args)
    {
        Calls.Add("ITestOutput");
    }

    void Visit(MessageHandlerArgs<ITestMethodCleanupFailure> args)
    {
        Calls.Add("ITestMethodCleanupFailure");
    }

    void Visit(MessageHandlerArgs<ITestExecutionSummary> args)
    {
        Calls.Add("ITestExecutionSummary");
    }

    void Visit(MessageHandlerArgs<ITestCleanupFailure> args)
    {
        Calls.Add("ITestCleanupFailure");
    }

    void Visit(MessageHandlerArgs<ITestClassCleanupFailure> args)
    {
        Calls.Add("ITestClassCleanupFailure");
    }

    void Visit(MessageHandlerArgs<ITestCaseCleanupFailure> args)
    {
        Calls.Add("ITestCaseCleanupFailure");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyExecutionStarting> args)
    {
        Calls.Add("ITestAssemblyExecutionStarting");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyExecutionFinished> args)
    {
        Calls.Add("ITestAssemblyExecutionFinished");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyDiscoveryStarting> args)
    {
        Calls.Add("ITestAssemblyDiscoveryStarting");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyDiscoveryFinished> args)
    {
        Calls.Add("ITestAssemblyDiscoveryFinished");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
    {
        Calls.Add("ITestAssemblyCleanupFailure");
    }

    void Visit(MessageHandlerArgs<IDiagnosticMessage> args)
    {
        Calls.Add("IDiagnosticMessage");
    }

    void Visit(MessageHandlerArgs<IAfterTestFinished> args)
    {
        Calls.Add("IAfterTestFinished");
    }

    void Visit(MessageHandlerArgs<IAfterTestStarting> args)
    {
        Calls.Add("IAfterTestStarting");
    }

    void Visit(MessageHandlerArgs<IBeforeTestFinished> args)
    {
        Calls.Add("IBeforeTestFinished");
    }

    void Visit(MessageHandlerArgs<IBeforeTestStarting> args)
    {
        Calls.Add("IBeforeTestStarting");
    }

    void Visit(MessageHandlerArgs<IDiscoveryCompleteMessage> args)
    {
        Calls.Add("IDiscoveryCompleteMessage");
    }

    void Visit(MessageHandlerArgs<IErrorMessage> args)
    {
        Calls.Add("IErrorMessage");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyFinished> args)
    {
        Calls.Add("ITestAssemblyFinished");
    }

    void Visit(MessageHandlerArgs<ITestAssemblyStarting> args)
    {
        Calls.Add("ITestAssemblyStarting");
    }

    void Visit(MessageHandlerArgs<ITestCaseDiscoveryMessage> args)
    {
        Calls.Add("ITestCaseDiscoveryMessage");
    }

    void Visit(MessageHandlerArgs<ITestCaseFinished> args)
    {
        Calls.Add("ITestCaseFinished");
    }

    void Visit(MessageHandlerArgs<ITestCaseStarting> args)
    {
        Calls.Add("ITestCaseStarting");
    }

    void Visit(MessageHandlerArgs<ITestClassConstructionFinished> args)
    {
        Calls.Add("ITestClassConstructionFinished");
    }

    void Visit(MessageHandlerArgs<ITestClassConstructionStarting> args)
    {
        Calls.Add("ITestClassConstructionStarting");
    }

    void Visit(MessageHandlerArgs<ITestClassDisposeFinished> args)
    {
        Calls.Add("ITestClassDisposeFinished");
    }

    void Visit(MessageHandlerArgs<ITestClassDisposeStarting> args)
    {
        Calls.Add("ITestClassDisposeStarting");
    }

    void Visit(MessageHandlerArgs<ITestClassFinished> args)
    {
        Calls.Add("ITestClassFinished");
    }

    void Visit(MessageHandlerArgs<ITestClassStarting> args)
    {
        Calls.Add("ITestClassStarting");
    }

    void Visit(MessageHandlerArgs<ITestCollectionFinished> args)
    {
        Calls.Add("ITestCollectionFinished");
    }

    void Visit(MessageHandlerArgs<ITestCollectionStarting> args)
    {
        Calls.Add("ITestCollectionStarting");
    }

    void Visit(MessageHandlerArgs<ITestFailed> args)
    {
        Calls.Add("ITestFailed");
    }

    void Visit(MessageHandlerArgs<ITestFinished> args)
    {
        Calls.Add("ITestFinished");
    }

    void Visit(MessageHandlerArgs<ITestMethodFinished> args)
    {
        Calls.Add("ITestMethodFinished");
    }

    void Visit(MessageHandlerArgs<ITestMethodStarting> args)
    {
        Calls.Add("ITestMethodStarting");
    }

    void Visit(MessageHandlerArgs<ITestPassed> args)
    {
        Calls.Add("ITestPassed");
    }

    void Visit(MessageHandlerArgs<ITestSkipped> args)
    {
        Calls.Add("ITestSkipped");
    }

    void Visit(MessageHandlerArgs<ITestStarting> args)
    {
        Calls.Add("ITestStarting");
    }
}