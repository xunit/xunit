using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SpyMessageBus : LongLivedMarshalByRefObject, IMessageBus
{
    readonly Func<IMessageSinkMessage, bool> cancellationThunk;

    public SpyMessageBus(Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        this.cancellationThunk = cancellationThunk ?? (msg => true);
    }

    public List<IMessageSinkMessage> Messages = new List<IMessageSinkMessage>();

    public void Dispose() { }

    public bool QueueMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);
        return cancellationThunk(message);
    }
}
