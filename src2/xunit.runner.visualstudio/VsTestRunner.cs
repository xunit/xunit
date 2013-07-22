using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Abstractions;

namespace Xunit.Runner.VisualStudio
{
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    [ExtensionUri(Constants.ExecutorUri)]
    public class VsTestRunner : ITestDiscoverer, ITestExecutor
    {
        public static TestProperty SerializedTestCaseProperty = GetTestProperty();
        static readonly ISourceInformationProvider SourceInformationProvider = new VisualStudioSourceInformationProvider();

        bool cancelled;

        public void Cancel()
        {
            cancelled = true;
        }

        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("logger", logger);
            Guard.ArgumentNotNull("discoverySink", discoverySink);

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();
                List<SourceSink> sourceSinks = new List<SourceSink>();

                using (AssemblyHelper.SubscribeResolve())
                    try
                    {
                        foreach (string source in sources)
                        {
                            try
                            {
                                if (cancelled)
                                    break;

                                if (IsXunitTestAssembly(source))
                                {
                                    var framework = new Xunit2(SourceInformationProvider, source, configFileName: null, shadowCopy: true);
                                    var sink = new VsDiscoveryVisitor(source, framework, logger, discoverySink, () => cancelled);
                                    sourceSinks.Add(new SourceSink { Framework = framework, Source = source, Sink = sink });
                                    framework.Find(includeSourceInformation: true, messageSink: sink);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.SendMessage(TestMessageLevel.Error, String.Format("xUnit.net: Exception discovering tests from {0}: {1}", source, e));
                            }

                        }

                        foreach (var sourceSink in sourceSinks)
                            sourceSink.Sink.Finished.WaitOne();
                    }
                    finally
                    {
                        foreach (var sourceSink in sourceSinks)
                        {
                            sourceSink.Sink.Dispose();
                            sourceSink.Framework.Dispose();
                        }
                    }
            }
            catch
            {
            }
        }

        private static TestProperty GetTestProperty()
        {
            return TestProperty.Register("XunitTestCase", "xUnit.net Test Case", typeof(string), typeof(VsTestRunner));
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");
            return File.Exists(xunitPath);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            var toDispose = new List<IDisposable>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                    foreach (string source in sources)
                        if (VsTestRunner.IsXunitTestAssembly(source))
                            RunTestsInAssembly(frameworkHandle, toDispose, source);
            }
            finally
            {
                Thread.Sleep(1000);

                foreach (var disposable in toDispose)
                    disposable.Dispose();
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("tests", tests);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            var toDispose = new List<IDisposable>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                    foreach (var testCaseGroup in tests.GroupBy(tc => tc.Source))
                        if (VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                            RunTestsInAssembly(frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup);
            }
            finally
            {
                Thread.Sleep(1000);

                foreach (var disposable in toDispose)
                    disposable.Dispose();
            }
        }

        void RunTestsInAssembly(IFrameworkHandle frameworkHandle, List<IDisposable> toDispose, string assemblyFileName, IEnumerable<TestCase> testCases = null)
        {
            if (cancelled)
                return;

            var xunit2 = new Xunit2(SourceInformationProvider, assemblyFileName, configFileName: null, shadowCopy: true);
            toDispose.Add(xunit2);

            if (testCases == null)
            {
                using (var visitor = new TestDiscoveryVisitor())
                {
                    xunit2.Find(includeSourceInformation: true, messageSink: visitor);
                    visitor.Finished.WaitOne();
                    testCases = visitor.TestCases.Select(tc => VsDiscoveryVisitor.CreateVsTestCase(frameworkHandle, assemblyFileName, xunit2, tc)).ToList();
                }
            }

            var xunitTestCases = testCases.ToDictionary(tc => xunit2.Deserialize(tc.GetPropertyValue<string>(SerializedTestCaseProperty, null)));

            using (var executionVisitor = new VsExecutionVisitor(assemblyFileName, frameworkHandle, xunitTestCases, () => cancelled))
            {
                xunit2.Run(xunitTestCases.Keys.ToList(), executionVisitor);
                executionVisitor.Finished.WaitOne();
            }
        }

        class SourceSink
        {
            public Xunit2 Framework;
            public string Source;
            public VsDiscoveryVisitor Sink;
        }
    }
}