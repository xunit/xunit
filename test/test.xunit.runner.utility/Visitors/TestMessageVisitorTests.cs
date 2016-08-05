#pragma warning disable CS0618

using System;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class TestMessageVisitorTests
{
    static readonly MethodInfo forMethodGeneric = typeof(Substitute).GetMethods().Single(m => m.Name == "For" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

    [Theory]
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
        var visitor = new SpyTestMessageVisitor();

        visitor.OnMessage(substitute);

        Assert.Collection(visitor.Calls,
            msg => Assert.Equal(type.Name, msg)
        );
    }

    [Fact]
    public void FinishedEventNotSignaledByDefault()
    {
        var visitor = new TestMessageVisitor<IMessageSinkMessage>();

        Assert.False(visitor.Finished.WaitOne(0));
    }

    [Fact]
    public void SignalsEventWhenMessageOfSpecifiedTypeIsSeen()
    {
        var visitor = new TestMessageVisitor<IDiscoveryCompleteMessage>();
        var message1 = Substitute.For<IMessageSinkMessage>();
        var message2 = Substitute.For<IDiscoveryCompleteMessage>();

        visitor.OnMessage(message1);
        Assert.False(visitor.Finished.WaitOne(0));

        visitor.OnMessage(message2);
        Assert.True(visitor.Finished.WaitOne(0));
    }
}
