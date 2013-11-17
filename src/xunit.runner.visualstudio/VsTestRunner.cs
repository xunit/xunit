using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Xunit.Runner.VisualStudio
{
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    [ExtensionUri(Constants.ExecutorUri)]
    public class VsTestRunner : ITestDiscoverer, ITestExecutor
    {
        public static TestProperty SerializedTestCaseProperty = GetTestProperty();

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
                                    var framework = new XunitFrontController(source, configFileName: null, shadowCopy: true);
                                    var sink = new VsDiscoveryVisitor(source, framework, logger, discoverySink, () => cancelled);
                                    sourceSinks.Add(new SourceSink { Framework = framework, Sink = sink });
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
            catch (Exception e)
            {
                logger.SendMessage(TestMessageLevel.Error, String.Format("xUnit.net: Exception discovering tests: {0}", e));
            }
        }

        static TestProperty GetTestProperty()
        {
            return TestProperty.Register("XunitTestCase", "xUnit.net Test Case", typeof(string), typeof(VsTestRunner));
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            string xunit1Path = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            string xunit2Path = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");
            return File.Exists(xunit1Path) || File.Exists(xunit2Path);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;

            var toDispose = new ConcurrentBag<IDisposable>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                {
                    var settings = SettingsProvider.Load();

                    if (settings.ParallelizeAssemblies)
                    {
                        var tasks = sources.Where(source => VsTestRunner.IsXunitTestAssembly(source))
                                           .Select(source => Task.Run(() => RunTestsInAssembly(frameworkHandle, toDispose, source)));
                        Task.WhenAll(tasks).GetAwaiter().GetResult();
                    }
                    else
                    {
                        foreach (string source in sources)
                            if (VsTestRunner.IsXunitTestAssembly(source))
                                RunTestsInAssembly(frameworkHandle, toDispose, source);
                    }
                }
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

            if (runContext.KeepAlive)
                frameworkHandle.EnableShutdownAfterTestRun = true;

            var toDispose = new ConcurrentBag<IDisposable>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                {
                    var settings = SettingsProvider.Load();

                    if (settings.ParallelizeAssemblies)
                    {
                        var tasks = tests.GroupBy(testCase => testCase.Source)
                                         .Where(testCaseGroup => VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                                         .Select(testCaseGroup => Task.Run(() => RunTestsInAssembly(frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup)));
                        Task.WhenAll(tasks).GetAwaiter().GetResult();
                    }
                    else
                    {
                        foreach (var testCaseGroup in tests.GroupBy(testCase => testCase.Source))
                            if (VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                                RunTestsInAssembly(frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup);
                    }
                }
            }
            finally
            {
                // This is to work around a race condition between when Visual Studio wants to "clean up" and when
                // our test app domains are ready to shut down. *facepalm*
                Thread.Sleep(1000);

                foreach (var disposable in toDispose)
                    disposable.Dispose();
            }
        }

        void RunTestsInAssembly(IFrameworkHandle frameworkHandle, ConcurrentBag<IDisposable> toDispose, string assemblyFileName, IEnumerable<TestCase> testCases = null)
        {
            if (cancelled)
                return;

            var controller = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true);
            toDispose.Add(controller);

            if (testCases == null)
                using (var visitor = new TestDiscoveryVisitor())
                {
                    controller.Find(includeSourceInformation: true, messageSink: visitor);
                    visitor.Finished.WaitOne();

                    var settings = SettingsProvider.Load();
                    testCases = visitor.TestCases.Select(tc => VsDiscoveryVisitor.CreateVsTestCase(assemblyFileName, controller, tc, settings)).ToList();
                }

            var xunitTestCases = testCases.ToDictionary(tc => controller.Deserialize(tc.GetPropertyValue<string>(SerializedTestCaseProperty, null)));

            using (var executionVisitor = new VsExecutionVisitor(assemblyFileName, frameworkHandle, xunitTestCases, () => cancelled))
            {
                controller.Run(xunitTestCases.Keys.ToList(), executionVisitor);
                executionVisitor.Finished.WaitOne();
            }
        }

        class SourceSink
        {
            public XunitFrontController Framework;
            public VsDiscoveryVisitor Sink;
        }
    }
}