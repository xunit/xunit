using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class AcceptanceTest : IDisposable
{
    protected Xunit2 Xunit2 { get; private set; }

    public void Dispose()
    {
        if (Xunit2 != null)
            Xunit2.Dispose();
    }

    public List<ITestMessage> Run(Type type)
    {
        Xunit2 = new Xunit2(new Uri(type.Assembly.CodeBase).LocalPath, configFileName: null, shadowCopy: true);
        var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();

        Xunit2.Find(type.FullName, includeSourceInformation: false, messageSink: discoverySink);
        discoverySink.Finished.WaitOne();

        var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

        var runSink = new SpyMessageSink<ITestAssemblyFinished>();

        Xunit2.Run(testCases, runSink);
        runSink.Finished.WaitOne();

        return runSink.Messages.ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type type)
        where TMessageType : ITestMessage
    {
        return Run(type).OfType<TMessageType>().ToList();
    }
}