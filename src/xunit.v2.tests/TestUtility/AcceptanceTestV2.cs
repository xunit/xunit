#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class AcceptanceTestV2 : IDisposable
{
    protected Xunit2 Xunit2 { get; private set; }

    public void Dispose()
    {
        if (Xunit2 != null)
            Xunit2.Dispose();
    }

    public List<IMessageSinkMessage> Run(Type type)
    {
        return Run(new[] { type });
    }

    public List<IMessageSinkMessage> Run(Type[] types)
    {
        Xunit2 = new Xunit2(AppDomainSupport.Required, new NullSourceInformationProvider(), types[0].Assembly.GetLocalCodeBase(), configFileName: null, shadowCopy: true);

        var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
        foreach (var type in types)
        {
            Xunit2.Find(type.FullName, includeSourceInformation: false, messageSink: discoverySink, discoveryOptions: TestFrameworkOptions.ForDiscovery());
            discoverySink.Finished.WaitOne();
            discoverySink.Finished.Reset();
        }

        var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

        var runSink = new SpyMessageSink<ITestAssemblyFinished>();
        Xunit2.RunTests(testCases, runSink, TestFrameworkOptions.ForExecution());
        runSink.Finished.WaitOne();

        return runSink.Messages.ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type type)
        where TMessageType : IMessageSinkMessage
    {
        return Run(type).OfType<TMessageType>().ToList();
    }

    public List<TMessageType> Run<TMessageType>(Type[] types)
        where TMessageType : IMessageSinkMessage
    {
        return Run(types).OfType<TMessageType>().ToList();
    }
}

#endif
