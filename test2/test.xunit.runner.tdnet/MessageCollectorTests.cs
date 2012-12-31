using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class MessageCollectorTests
{
    [Fact]
    public void FinishedEventNotSignaledByDefault()
    {
        var collector = new MessageCollector<ITestMessage>();

        Assert.False(collector.Finished.WaitOne(0));
    }

    [Fact]
    public void CollectsMessagesPassedToOnMessage()
    {
        var collector = new MessageCollector<ITestMessage>();
        var message1 = new Mock<ITestMessage>().Object;
        var message2 = new Mock<ITestMessage>().Object;
        var message3 = new Mock<ITestMessage>().Object;

        collector.OnMessage(message1);
        collector.OnMessage(message2);
        collector.OnMessage(message3);

        Assert.Collection(collector.Messages,
            msg => Assert.Same(message1, msg),
            msg => Assert.Same(message2, msg),
            msg => Assert.Same(message3, msg)
        );
    }

    [Fact]
    public void SignalsEventWhenMessageOfSpecifiedTypeIsSeen()
    {
        var collector = new MessageCollector<IDiscoveryCompleteMessage>();
        var message1 = new Mock<ITestMessage>().Object;
        var message2 = new Mock<IDiscoveryCompleteMessage>().Object;

        collector.OnMessage(message1);
        Assert.False(collector.Finished.WaitOne(0));

        collector.OnMessage(message2);
        Assert.True(collector.Finished.WaitOne(0));
    }
}