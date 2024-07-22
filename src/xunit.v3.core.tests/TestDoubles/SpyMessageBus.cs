using System;
using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.v3;

public class SpyMessageBus(Func<IMessageSinkMessage, bool>? cancellationThunk = null) :
	IMessageBus
{
	readonly Func<IMessageSinkMessage, bool> cancellationThunk = cancellationThunk ?? (msg => true);
	public List<IMessageSinkMessage> Messages = [];

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public bool QueueMessage(IMessageSinkMessage message)
	{
		Messages.Add(message);
		return cancellationThunk(message);
	}
}
