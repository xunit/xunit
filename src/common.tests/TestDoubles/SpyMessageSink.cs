using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

public class SpyMessageSink<TFinalMessage> : LongLivedMarshalByRefObject, IMessageSink
{
    readonly Func<IMessageSinkMessage, bool> cancellationThunk;

    public SpyMessageSink(Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        this.cancellationThunk = cancellationThunk ?? (msg => true);
    }

    public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

    public List<IMessageSinkMessage> Messages = new List<IMessageSinkMessage>();

    /// <inheritdoc/>
    public void Dispose()
    {
        Finished.Dispose();
    }

    public bool OnMessage(IMessageSinkMessage message)
    {
        Messages.Add(message);

        if (message is TFinalMessage)
            Finished.Set();

        return cancellationThunk(message);
    }
}
