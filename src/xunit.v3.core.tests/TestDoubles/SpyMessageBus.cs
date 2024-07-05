using System;
using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.v3;

public class SpyMessageBus(Func<MessageSinkMessage, bool>? cancellationThunk = null) :
	IMessageBus
{
	readonly Func<MessageSinkMessage, bool> cancellationThunk = cancellationThunk ?? (msg => true);
	public List<MessageSinkMessage> Messages = [];

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public bool QueueMessage(MessageSinkMessage message)
	{
		Messages.Add(message);
		return cancellationThunk(message);
	}
}
