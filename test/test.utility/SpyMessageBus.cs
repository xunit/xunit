using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SpyMessageBus<TFinalMessage> : LongLivedMarshalByRefObject, IMessageBus
{
    readonly Func<IMessageSinkMessage, bool> cancellationThunk;

    public SpyMessageBus(Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        this.cancellationThunk = cancellationThunk ?? (msg => true);
    }

    public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

    public List<IMessageSinkMessage> Messages = new List<IMessageSinkMessage>();

    /// <inheritdoc/>
    public void Dispose() { }

    public bool QueueMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);

        if (message is TFinalMessage)
            Finished.Set();

        return cancellationThunk(message);
    }
}
