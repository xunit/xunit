using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class MessageBusTests
{
    IMessageSink SpySink(List<ITestMessage> messages = null)
    {
        var result = Substitute.For<IMessageSink>();

        result.OnMessage(null).ReturnsForAnyArgs(
            callInfo =>
            {
                if (messages != null)
                    messages.Add((ITestMessage)callInfo[0]);

                return true;
            });

        return result;
    }

    [Fact]
    public void QueuedMessageShowUpInMessageSink()
    {
        var messages = new List<ITestMessage>();
        var sink = SpySink(messages);
        var msg1 = Substitute.For<ITestMessage>();
        var msg2 = Substitute.For<ITestMessage>();
        var msg3 = Substitute.For<ITestMessage>();

        using (var bus = new MessageBus(sink))
        {
            bus.QueueMessage(msg1);
            bus.QueueMessage(msg2);
            bus.QueueMessage(msg3);
        }

        Assert.Collection(messages,
            message => Assert.Same(msg1, message),
            message => Assert.Same(msg2, message),
            message => Assert.Same(msg3, message)
        );
    }

    [Fact]
    public void DisposingMessageBusDisposesMessageSink()
    {
        var sink = SpySink();

        new MessageBus(sink).Dispose();

        sink.Received(1).Dispose();
    }

    [Fact]
    public void TryingToQueueMessageAfterDisposingThrows()
    {
        var bus = new MessageBus(SpySink());
        bus.Dispose();

        var exception = Record.Exception(
            () => bus.QueueMessage(Substitute.For<ITestMessage>())
        );

        Assert.IsType<ObjectDisposedException>(exception);
    }

    [Fact]
    public void WhenSinkThrowsMessagesContinueToBeDelivered()
    {
        var sink = Substitute.For<IMessageSink>();
        var msg1 = Substitute.For<ITestMessage>();
        var msg2 = Substitute.For<ITestMessage>();
        var msg3 = Substitute.For<ITestMessage>();
        var messages = new List<ITestMessage>();
        sink.OnMessage(Arg.Any<ITestMessage>())
            .Returns(callInfo =>
            {
                var msg = (ITestMessage)callInfo[0];
                if (msg == msg2)
                    throw new Exception("whee!");
                else
                    messages.Add(msg);

                return false;
            });

        using (var bus = new MessageBus(sink))
        {
            bus.QueueMessage(msg1);
            bus.QueueMessage(msg2);
            bus.QueueMessage(msg3);
        }

        Assert.Collection(messages,
            message => Assert.Same(message, msg1),
            message => Assert.Same(message, msg3)
        );
    }
}
