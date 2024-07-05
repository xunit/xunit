using System;
using System.Collections.Generic;
using Xunit.Sdk;
using Xunit.v3;

public class SpyMessageBus(Func<_MessageSinkMessage, bool>? cancellationThunk = null) :
	IMessageBus
{
	readonly Func<_MessageSinkMessage, bool> cancellationThunk = cancellationThunk ?? (msg => true);
	public List<_MessageSinkMessage> Messages = [];

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	public bool QueueMessage(_MessageSinkMessage message)
	{
		Messages.Add(message);
		return cancellationThunk(message);
	}
}
