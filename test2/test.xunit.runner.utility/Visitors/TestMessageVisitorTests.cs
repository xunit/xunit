using System;
using System.Reflection;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;

public class TestMessageVisitorTests
{
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
        // Make a mock of the interface in question
        var mockType = typeof(Mock<>).MakeGenericType(type);
        var ctor = mockType.GetConstructor(new Type[0]);
        var mock = (Mock)ctor.Invoke(new object[0]);
        var mockImplementation = (ITestMessage)mock.Object;

        // Make the expression for the method we expect to be called
        var isAny = typeof(ItExpr).GetMethod("IsAny", BindingFlags.Static | BindingFlags.Public)
                                  .MakeGenericMethod(new[] { type });
        var expression = isAny.Invoke(null, new object[0]);

        // Make a mock of the visitor so we can verify the right method was called
        var mockVisitor = new Mock<TestMessageVisitor> { CallBase = true };
        mockVisitor.Protected().Setup("Visit", expression).Verifiable();

        // Call the visitor with the mock
        mockVisitor.Object.OnMessage(mockImplementation);

        // Assert
        mockVisitor.Verify();
    }

    [Fact]
    public void FinishedEventNotSignaledByDefault()
    {
        var visitor = new Mock<TestMessageVisitor<ITestMessage>> { CallBase = true }.Object;

        Assert.False(visitor.Finished.WaitOne(0));
    }

    [Fact]
    public void SignalsEventWhenMessageOfSpecifiedTypeIsSeen()
    {
        var visitor = new Mock<TestMessageVisitor<IDiscoveryCompleteMessage>> { CallBase = true }.Object;
        var message1 = new Mock<ITestMessage>().Object;
        var message2 = new Mock<IDiscoveryCompleteMessage>().Object;

        visitor.OnMessage(message1);
        Assert.False(visitor.Finished.WaitOne(0));

        visitor.OnMessage(message2);
        Assert.True(visitor.Finished.WaitOne(0));
    }
}