using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SynchronousMessageBusTests
{
    [Fact]
    public void MessagesAreDispatchedImmediatelyFromBus()
    {
        var msg1 = Substitute.For<IMessageSinkMessage>();
        var dispatchedMessages = new List<IMessageSinkMessage>();
        using (var bus = new SynchronousMessageBus(SpyMessageSink.Create(messages: dispatchedMessages), stopOnFail: false))
        {
            Assert.True(bus.QueueMessage(msg1));
        }

        Assert.Collection(dispatchedMessages, message => Assert.Same(msg1, message));
    }

    [Fact]
    public void BusShouldReportShutdownWhenMessageSinkReturnsFalse()
    {
        using var bus = new SynchronousMessageBus(SpyMessageSink.Create(returnResult: false), stopOnFail: false);

        Assert.False(bus.QueueMessage(Substitute.For<IMessageSinkMessage>()));
    }

    [Fact]
    public void WhenStopOnFailIsFalse_WithFailedTest_BusShouldNotReportShutdown()
    {
        var testPassedMessage = Substitute.For<ITestPassed>();
        var testFailedMessage = Substitute.For<ITestFailed>();
        using var bus = new SynchronousMessageBus(SpyMessageSink.Create(), stopOnFail: false);

        Assert.True(bus.QueueMessage(testPassedMessage));
        Assert.True(bus.QueueMessage(testFailedMessage));
        Assert.True(bus.QueueMessage(testPassedMessage));
    }

    [Fact]
    public void WhenStopOnFailIsTrue_WithFailedTest_BusShouldReportShutdown()
    {
        var testPassedMessage = Substitute.For<ITestPassed>();
        var testFailedMessage = Substitute.For<ITestFailed>();
        using var bus = new SynchronousMessageBus(SpyMessageSink.Create(), stopOnFail: true);

        Assert.True(bus.QueueMessage(testPassedMessage));
        Assert.False(bus.QueueMessage(testFailedMessage));
        Assert.False(bus.QueueMessage(testPassedMessage));
    }
}
