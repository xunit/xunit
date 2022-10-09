using System;
using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.v3;

public class SpyMessageBus : IMessageBus
{
	readonly Func<_MessageSinkMessage, bool> cancellationThunk;

	public SpyMessageBus(Func<_MessageSinkMessage, bool>? cancellationThunk = null)
	{
		this.cancellationThunk = cancellationThunk ?? (msg => true);
	}

	public List<_MessageSinkMessage> Messages = new List<_MessageSinkMessage>();

	public void Dispose()
	{ }

	public bool QueueMessage(_MessageSinkMessage message)
	{
		Messages.Add(message);
		return cancellationThunk(message);
	}
}
