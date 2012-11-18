using System;
using System.Collections.Generic;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class MessageBusTests
{
    [Fact]
    public void QueuedMessageShowUpInMessageSink()
    {
        var spy = new SpyMessageSink();
        var msg1 = new Mock<ITestMessage>();
        var msg2 = new Mock<ITestMessage>();
        var msg3 = new Mock<ITestMessage>();

        using (var bus = new MessageBus(spy.Object))
        {
            bus.QueueMessage(msg1.Object);
            bus.QueueMessage(msg2.Object);
            bus.QueueMessage(msg3.Object);
        }

        CollectionAssert.Collection(spy.Messages,
            message => Assert.Same(msg1.Object, message),
            message => Assert.Same(msg2.Object, message),
            message => Assert.Same(msg3.Object, message)
        );
    }

    [Fact]
    public void DisposingMessageBusDisposesMessageSink()
    {
        var spy = new SpyMessageSink();

        using (var bus = new MessageBus(spy.Object)) { }

        spy.Verify(s => s.Dispose());
    }

    [Fact]
    public void TryingToQueueMessageAfterDisposingThrows()
    {
        var spy = new SpyMessageSink();
        var bus = new MessageBus(spy.Object);
        bus.Dispose();

        var exception = Record.Exception(
            () => bus.QueueMessage(new Mock<ITestMessage>().Object)
        );

        Assert.IsType<ObjectDisposedException>(exception);
    }

    [Fact]
    public void WhenSinkThrowsMessagesContinueToBeDelivered()
    {
        var spy = new Mock<IMessageSink>();
        var msg1 = new Mock<ITestMessage>();
        var msg2 = new Mock<ITestMessage>();
        var msg3 = new Mock<ITestMessage>();
        var messages = new List<ITestMessage>();
        spy.Setup(s => s.OnMessage(It.IsAny<ITestMessage>()))
           .Callback<ITestMessage>(msg =>
           {
               if (msg == msg2.Object)
                   throw new Exception("whee!");
               else
                   messages.Add(msg);
           });

        using (var bus = new MessageBus(spy.Object))
        {
            bus.QueueMessage(msg1.Object);
            bus.QueueMessage(msg2.Object);
            bus.QueueMessage(msg3.Object);
        }

        CollectionAssert.Collection(messages,
            message => Assert.Same(message, msg1.Object),
            message => Assert.Same(message, msg3.Object)
        );
    }

    class CompletionMessage : ITestMessage { }

    class SpyMessageSink : Mock<IMessageSink>
    {
        public List<ITestMessage> Messages = new List<ITestMessage>();

        public SpyMessageSink()
        {
            this.Setup(sink => sink.OnMessage(It.IsAny<ITestMessage>()))
                .Callback<ITestMessage>(Messages.Add);
        }
    }
}
