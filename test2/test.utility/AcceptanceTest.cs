using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class Xunit2AcceptanceTest : IDisposable
{
    protected Xunit2Controller Controller { get; private set; }

    public void Dispose()
    {
        if (Controller != null)
            Controller.Dispose();
    }

    public List<ITestMessage> Run(Type type)
    {
        Controller = new Xunit2Controller(new Uri(type.Assembly.CodeBase).LocalPath, configFileName: null, shadowCopy: true);
        var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();

        Controller.Find(Reflector2.Wrap(type), includeSourceInformation: false, messageSink: discoverySink);
        discoverySink.Finished.WaitOne();

        var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

        var runSink = new SpyMessageSink<ITestAssemblyFinished>();

        Controller.Run(testCases, runSink);
        runSink.Finished.WaitOne();

        return runSink.Messages.ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type type)
        where TMessageType : ITestMessage
    {
        return Run(type).OfType<TMessageType>().ToList();
    }
}