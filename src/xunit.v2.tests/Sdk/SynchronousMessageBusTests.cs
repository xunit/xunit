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

		var message = Assert.Single(dispatchedMessages);
		Assert.Same(msg1, message);
	}

	[Fact]
	public void BusShouldReportShutdownWhenMessageSinkReturnsFalse()
	{
		using (var bus = new SynchronousMessageBus(SpyMessageSink.Create(returnResult: false), stopOnFail: false))
		{
			Assert.False(bus.QueueMessage(Substitute.For<IMessageSinkMessage>()));
		}
	}
}
