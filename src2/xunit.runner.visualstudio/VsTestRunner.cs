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

                using (AssemblyHelper.SubscribeResolve())
                {
                    foreach (string source in sources)
                        try
                        {
                            if (cancelled)
                                break;

                            if (IsXunitTestAssembly(source))
                                using (var xunit2 = new Xunit2(source, configFileName: null, shadowCopy: true))
                                using (var sink = new VsDiscoveryVisitor(source, discoverySink, () => cancelled))
                                {
                                    TestCaseMapper.Clear(source);
                                    xunit2.Find(includeSourceInformation: true, messageSink: sink);
                                    sink.Finished.WaitOne();
                                }
                        }
                        catch (Exception e)
                        {
                            logger.SendMessage(TestMessageLevel.Error, String.Format("xUnit.net: Exception discovering tests from {0}: {1}", source, e));
                        }
                }
            }
            catch
            {
            }
        }

        static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {
            return displayName == fullyQualifiedMethodName ? shortMethodName : displayName;
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");
            return File.Exists(xunitPath);
        }

        private static TestProperty RegisterTestCaseTestProperty()
        {
            return TestProperty.Register("XunitTestCase", "XunitTestCase", typeof(ITestCase), typeof(VsTestRunner));
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

                foreach (string source in sources)
                    if (VsTestRunner.IsXunitTestAssembly(source))
                        RunTestsInAssembly(toDispose, source, frameworkHandle);
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

                foreach (var testCaseGroup in tests.GroupBy(tc => tc.Source))
                    if (VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                        RunTestsInAssembly(toDispose, testCaseGroup.Key, frameworkHandle, testCaseGroup);
            }
            finally
            {
                Thread.Sleep(1000);

                foreach (var disposable in toDispose)
                    disposable.Dispose();
            }
        }

        void RunTestsInAssembly(List<IDisposable> toDispose, string assemblyFileName, ITestExecutionRecorder recorder, IEnumerable<TestCase> testCases = null)
        {
            if (cancelled)
                return;

            var xunit2 = new Xunit2(assemblyFileName, configFileName: null, shadowCopy: true);
            toDispose.Add(xunit2);

            if (testCases == null)
            {
                using (var visitor = new TestDiscoveryVisitor())
                {
                    xunit2.Find(includeSourceInformation: true, messageSink: visitor);
                    visitor.Finished.WaitOne();
                    testCases = visitor.TestCases.Select(tc => VsDiscoveryVisitor.CreateVsTestCase(assemblyFileName, tc)).ToList();
                }
            }

            using (var executionVisitor = new VsExecutionVisitor(assemblyFileName, recorder, testCases, () => cancelled))
            {
                xunit2.Run(testCases.Select(tc => TestCaseMapper.Find(assemblyFileName, tc)).ToList(), executionVisitor);
                executionVisitor.Finished.WaitOne();
            }
        }
    }
}
