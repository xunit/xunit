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
    [InlineData(typeof(ITestCollectionCleanupFailure))]
    [InlineData(typeof(ITestOutput))]
    [InlineData(typeof(ITestMethodCleanupFailure))]
    [InlineData(typeof(ITestExecutionSummary))]
    [InlineData(typeof(ITestCleanupFailure))]
    [InlineData(typeof(ITestClassCleanupFailure))]
    [InlineData(typeof(ITestCaseCleanupFailure))]
    [InlineData(typeof(ITestAssemblyExecutionStarting))]
    [InlineData(typeof(ITestAssemblyExecutionFinished))]
    [InlineData(typeof(ITestAssemblyDiscoveryStarting))]
    [InlineData(typeof(ITestAssemblyDiscoveryFinished))]
    [InlineData(typeof(ITestAssemblyCleanupFailure))]
    [InlineData(typeof(IDiagnosticMessage))]
    [InlineData(typeof(IAfterTestFinished))]
    [InlineData(typeof(IAfterTestStarting))]
    [InlineData(typeof(IBeforeTestFinished))]
    [InlineData(typeof(IBeforeTestStarting))]
    [InlineData(typeof(IDiscoveryCompleteMessage))]
    [InlineData(typeof(IErrorMessage))]
    [InlineData(typeof(ITestAssemblyFinished))]
    [InlineData(typeof(ITestAssemblyStarting))]
    [InlineData(typeof(ITestCaseDiscoveryMessage))]
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
    public void ProcessesVisitorTypes(Type type)
    {
        var forMethod = forMethodGeneric.MakeGenericMethod(type);
        var substitute = (IMessageSinkMessage)forMethod.Invoke(null, new object[] { new object[0] });
        var visitor = new SpyTestMessageSink();

        visitor.OnMessageWithTypes(substitute, null);

        Assert.Collection(visitor.Calls,
            msg => Assert.Equal(type.Name, msg)
        );
    }
}
