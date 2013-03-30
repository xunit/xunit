using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SpyMessageSink<TFinalMessage> : LongLivedMarshalByRefObject, IMessageSink
{
    Func<ITestMessage, bool> cancellationThunk;

    public SpyMessageSink(Func<ITestMessage, bool> cancellationThunk = null)
    {
        this.cancellationThunk = cancellationThunk ?? (msg => true);
    }

    public ManualResetEvent Finished = new ManualResetEvent(initialState: false);

    public List<ITestMessage> Messages = new List<ITestMessage>();

    public bool OnMessage(ITestMessage message)
    {
        Messages.Add(message);

        if (message is TFinalMessage)
            Finished.Set();

        return cancellationThunk(message);
    }

    public void Dispose() { }
}
