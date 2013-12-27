using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class AcceptanceTest : IDisposable
{
    readonly List<IDisposable> ToDispose = new List<IDisposable>();

    protected Xunit2 Xunit2 { get; private set; }

    public void Dispose()
    {
        ToDispose.ForEach(d => d.Dispose());

        if (Xunit2 != null)
            Xunit2.Dispose();
    }

    public List<IMessageSinkMessage> Run(Type type, Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        return Run(new[] { type }, cancellationThunk);
    }

    public List<IMessageSinkMessage> Run(Type[] types, Func<IMessageSinkMessage, bool> cancellationThunk = null)
    {
        Xunit2 = new Xunit2(new NullSourceInformationProvider(), new Uri(types[0].Assembly.CodeBase).LocalPath, configFileName: null, shadowCopy: true);

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
            Xunit2.Find(type.FullName, includeSourceInformation: false, messageSink: discoverySink);
            discoverySink.Finished.WaitOne();
            discoverySink.Finished.Reset();
        }

        ToDispose.AddRange(discoverySink.Messages);

        if (cancelled)
            return new List<IMessageSinkMessage>();

        var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

        var runSink = new SpyMessageSink<ITestAssemblyFinished>(cancellationThunk);
        Xunit2.Run(testCases, runSink);
        runSink.Finished.WaitOne();
        ToDispose.AddRange(runSink.Messages);

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