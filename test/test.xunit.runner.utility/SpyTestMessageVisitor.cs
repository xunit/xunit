#pragma warning disable CS0618

using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

public class SpyTestMessageVisitor : TestMessageVisitor
{
    public List<string> Calls = new List<string>();

    protected override bool Visit(IAfterTestFinished afterTestFinished)
    {
        Calls.Add("IAfterTestFinished");
        return base.Visit(afterTestFinished);
    }

    protected override bool Visit(IAfterTestStarting afterTestStarting)
    {
        Calls.Add("IAfterTestStarting");
        return base.Visit(afterTestStarting);
    }

    protected override bool Visit(IBeforeTestFinished beforeTestFinished)
    {
        Calls.Add("IBeforeTestFinished");
        return base.Visit(beforeTestFinished);
    }

    protected override bool Visit(IBeforeTestStarting beforeTestStarting)
    {
        Calls.Add("IBeforeTestStarting");
        return base.Visit(beforeTestStarting);
    }

    protected override bool Visit(IDiscoveryCompleteMessage discoveryComplete)
    {
        Calls.Add("IDiscoveryCompleteMessage");
        return base.Visit(discoveryComplete);
    }

    protected override bool Visit(IErrorMessage error)
    {
        Calls.Add("IErrorMessage");
        return base.Visit(error);
    }

    protected override bool Visit(ITestAssemblyFinished assemblyFinished)
    {
        Calls.Add("ITestAssemblyFinished");
        return base.Visit(assemblyFinished);
    }

    protected override bool Visit(ITestAssemblyStarting assemblyStarting)
    {
        Calls.Add("ITestAssemblyStarting");
        return base.Visit(assemblyStarting);
    }

    protected override bool Visit(ITestCaseDiscoveryMessage testCaseDiscovered)
    {
        Calls.Add("ITestCaseDiscoveryMessage");
        return base.Visit(testCaseDiscovered);
    }

    protected override bool Visit(ITestCaseFinished testCaseFinished)
    {
        Calls.Add("ITestCaseFinished");
        return base.Visit(testCaseFinished);
    }

    protected override bool Visit(ITestCaseStarting testCaseStarting)
    {
        Calls.Add("ITestCaseStarting");
        return base.Visit(testCaseStarting);
    }

    protected override bool Visit(ITestClassConstructionFinished testClassConstructionFinished)
    {
        Calls.Add("ITestClassConstructionFinished");
        return base.Visit(testClassConstructionFinished);
    }

    protected override bool Visit(ITestClassConstructionStarting testClassConstructionStarting)
    {
        Calls.Add("ITestClassConstructionStarting");
        return base.Visit(testClassConstructionStarting);
    }

    protected override bool Visit(ITestClassDisposeFinished testClassDisposedFinished)
    {
        Calls.Add("ITestClassDisposeFinished");
        return base.Visit(testClassDisposedFinished);
    }

    protected override bool Visit(ITestClassDisposeStarting testClassDisposeStarting)
    {
        Calls.Add("ITestClassDisposeStarting");
        return base.Visit(testClassDisposeStarting);
    }

    protected override bool Visit(ITestClassFinished testClassFinished)
    {
        Calls.Add("ITestClassFinished");
        return base.Visit(testClassFinished);
    }

    protected override bool Visit(ITestClassStarting testClassStarting)
    {
        Calls.Add("ITestClassStarting");
        return base.Visit(testClassStarting);
    }

    protected override bool Visit(ITestCollectionFinished testCollectionFinished)
    {
        Calls.Add("ITestCollectionFinished");
        return base.Visit(testCollectionFinished);
    }

    protected override bool Visit(ITestCollectionStarting testCollectionStarting)
    {
        Calls.Add("ITestCollectionStarting");
        return base.Visit(testCollectionStarting);
    }

    protected override bool Visit(ITestFailed testFailed)
    {
        Calls.Add("ITestFailed");
        return base.Visit(testFailed);
    }

    protected override bool Visit(ITestFinished testFinished)
    {
        Calls.Add("ITestFinished");
        return base.Visit(testFinished);
    }

    protected override bool Visit(ITestMethodFinished testMethodFinished)
    {
        Calls.Add("ITestMethodFinished");
        return base.Visit(testMethodFinished);
    }

    protected override bool Visit(ITestMethodStarting testMethodStarting)
    {
        Calls.Add("ITestMethodStarting");
        return base.Visit(testMethodStarting);
    }

    protected override bool Visit(ITestPassed testPassed)
    {
        Calls.Add("ITestPassed");
        return base.Visit(testPassed);
    }

    protected override bool Visit(ITestSkipped testSkipped)
    {
        Calls.Add("ITestSkipped");
        return base.Visit(testSkipped);
    }

    protected override bool Visit(ITestStarting testStarting)
    {
        Calls.Add("ITestStarting");
        return base.Visit(testStarting);
    }
}
