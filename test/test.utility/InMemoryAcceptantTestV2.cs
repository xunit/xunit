using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using NullMessageSink = Xunit.NullMessageSink;

public static class InMemoryAcceptanceTestV2
{
    public static List<IMessageSinkMessage> Run(
        Type type,
        bool preEnumerateTheories = true,
        IMessageSink diagnosticMessageSink = null) =>
            Run(new[] { type }, preEnumerateTheories, diagnosticMessageSink);

    public static List<IMessageSinkMessage> Run(
        Type[] types,
        bool preEnumerateTheories = true,
        IMessageSink diagnosticMessageSink = null)
    {
        if (types == null || types.Length == 0)
            return new List<IMessageSinkMessage>();

        using var testFramework = new XunitTestFramework(diagnosticMessageSink ?? new NullMessageSink());

        var assemblyInfo = Reflector.Wrap(types[0].GetTypeInfo().Assembly);
        var discoverer = testFramework.GetDiscoverer(assemblyInfo);
        var discoveryOptions = TestFrameworkOptions.ForDiscovery();
        var testCases = new List<ITestCase>();
        discoveryOptions.SetPreEnumerateTheories(preEnumerateTheories);

        foreach (var type in types)
        {
            var discoverySink = new TestDiscoverySink();
            discoverer.Find(type.FullName, includeSourceInformation: false, discoverySink, discoveryOptions);
            discoverySink.Finished.WaitOne();

            testCases.AddRange(discoverySink.TestCases);
        }

        var executionSink = new SpyMessageSink<ITestAssemblyFinished>();
        var executionOptions = TestFrameworkOptions.ForExecution();
        var executor = testFramework.GetExecutor(types[0].GetTypeInfo().Assembly.GetName());
        executor.RunTests(testCases, executionSink, executionOptions);
        executionSink.Finished.WaitOne();

        return executionSink.Messages.ToList();
    }
}
