using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class AcceptanceTest : IDisposable
{
    protected Xunit2 Xunit2 { get; private set; }

    public void Dispose()
    {
        if (Xunit2 != null)
            Xunit2.Dispose();
    }

    public List<IMessageSinkMessage> Run(Type type, Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        return Run(new[] { type }, cancellationThunk);
    }

    public List<IMessageSinkMessage> Run(Type[] types, Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        Xunit2 = new Xunit2(new NullSourceInformationProvider(), types[0].Assembly.GetLocalCodeBase(), configFileName: null, shadowCopy: true);

        bool cancelled = false;
        Func<IMessageSinkMessage, bool> wrapper = msg =>
        {
            var result = true;

            if (cancellationThunk != null)
                result = cancellationThunk(msg);

            if (!result)
                cancelled = true;

            return result;
        };

        var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>(wrapper);
        foreach (Type type in types)
        {
            Xunit2.Find(type.FullName, includeSourceInformation: false, messageSink: discoverySink, options: new XunitDiscoveryOptions());
            discoverySink.Finished.WaitOne();
            discoverySink.Finished.Reset();
        }

        if (cancelled)
            return new List<IMessageSinkMessage>();

        var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

        var runSink = new SpyMessageSink<ITestAssemblyFinished>(cancellationThunk);
        Xunit2.Run(testCases, runSink, new XunitExecutionOptions());
        runSink.Finished.WaitOne();

        return runSink.Messages.ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type type, Func<IMessageSinkMessage, bool> cancellationThunk = null)
        where TMessageType : IMessageSinkMessage
    {
        return Run(type).OfType<TMessageType>().ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type[] types, Func<IMessageSinkMessage, bool> cancellationThunk = null)
        where TMessageType : IMessageSinkMessage
    {
        return Run(types).OfType<TMessageType>().ToList();
    }
}