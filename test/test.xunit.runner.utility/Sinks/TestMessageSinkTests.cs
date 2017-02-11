using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestMessageSinkTests
{
    static readonly MethodInfo forMethodGeneric = typeof(Substitute).GetMethods().Single(m => m.Name == nameof(Substitute.For) && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    [Theory]
    // Diagnostics
    [InlineData(typeof(IDiagnosticMessage))]
    [InlineData(typeof(IErrorMessage))]
    // Discovery
    [InlineData(typeof(IDiscoveryCompleteMessage))]
    [InlineData(typeof(ITestCaseDiscoveryMessage))]
    // Execution
    [InlineData(typeof(ITestCollectionCleanupFailure))]
    [InlineData(typeof(ITestOutput))]
    [InlineData(typeof(ITestMethodCleanupFailure))]
    [InlineData(typeof(ITestCleanupFailure))]
    [InlineData(typeof(ITestClassCleanupFailure))]
    [InlineData(typeof(ITestCaseCleanupFailure))]
    [InlineData(typeof(ITestAssemblyCleanupFailure))]
    [InlineData(typeof(IAfterTestFinished))]
    [InlineData(typeof(IAfterTestStarting))]
    [InlineData(typeof(IBeforeTestFinished))]
    [InlineData(typeof(IBeforeTestStarting))]
    [InlineData(typeof(ITestAssemblyFinished))]
    [InlineData(typeof(ITestAssemblyStarting))]
    [InlineData(typeof(ITestCaseFinished))]
    [InlineData(typeof(ITestCaseStarting))]
    [InlineData(typeof(ITestClassConstructionFinished))]
    [InlineData(typeof(ITestClassConstructionStarting))]
    [InlineData(typeof(ITestClassDisposeFinished))]
    [InlineData(typeof(ITestClassDisposeStarting))]
    [InlineData(typeof(ITestClassFinished))]
    [InlineData(typeof(ITestClassStarting))]
    [InlineData(typeof(ITestCollectionFinished))]
    [InlineData(typeof(ITestCollectionStarting))]
    [InlineData(typeof(ITestFailed))]
    [InlineData(typeof(ITestFinished))]
    [InlineData(typeof(ITestMethodFinished))]
    [InlineData(typeof(ITestMethodStarting))]
    [InlineData(typeof(ITestPassed))]
    [InlineData(typeof(ITestSkipped))]
    [InlineData(typeof(ITestStarting))]
    // Runner
    [InlineData(typeof(ITestAssemblyExecutionStarting))]
    [InlineData(typeof(ITestAssemblyExecutionFinished))]
    [InlineData(typeof(ITestAssemblyDiscoveryStarting))]
    [InlineData(typeof(ITestAssemblyDiscoveryFinished))]
    [InlineData(typeof(ITestExecutionSummary))]
    public void ProcessesVisitorTypes(Type type)
    {
        var forMethod = forMethodGeneric.MakeGenericMethod(type);
        var substitute = (IMessageSinkMessage)forMethod.Invoke(null, new object[] { new object[0] });
        var sink = new SpyTestMessageSink();

        sink.OnMessageWithTypes(substitute, null);

        Assert.Collection(sink.Calls,
            msg => Assert.Equal(type.Name, msg)
        );
    }
}
