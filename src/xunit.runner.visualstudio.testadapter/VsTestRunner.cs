using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Runner.VisualStudio.Settings;

namespace Xunit.Runner.VisualStudio.TestAdapter
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

            var settings = SettingsProvider.Load();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery started", stopwatch.Elapsed));

                using (AssemblyHelper.SubscribeResolve())
                {
                    foreach (string assemblyFileName in sources)
                    {
                        var fileName = Path.GetFileName(assemblyFileName);

                        try
                        {
                            if (cancelled)
                                break;

                            if (!IsXunitTestAssembly(assemblyFileName))
                            {
                                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Skipping: {1}", stopwatch.Elapsed, fileName));
                            }
                            else
                            {
                                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery starting: {1}", stopwatch.Elapsed, fileName));

                                using (var framework = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true))
                                using (var sink = new VsDiscoveryVisitor(assemblyFileName, framework, logger, discoveryContext, discoverySink, () => cancelled))
                                {
                                    framework.Find(includeSourceInformation: true, messageSink: sink, options: new TestFrameworkOptions());
                                    sink.Finished.WaitOne();

                                    if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                        logger.SendMessage(TestMessageLevel.Informational,
                                                           String.Format("[xUnit.net {0}] Discovery finished: {1} ({2} tests)", stopwatch.Elapsed, fileName, sink.TotalTests));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            logger.SendMessage(TestMessageLevel.Error,
                                               String.Format("[xUnit.net {0}] Exception discovering tests from {1}: {2}", stopwatch.Elapsed, fileName, e));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.SendMessage(TestMessageLevel.Error,
                                   String.Format("[xUnit.net {0}] Exception discovering tests: {1}", stopwatch.Elapsed, e));
            }

            stopwatch.Stop();

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery complete", stopwatch.Elapsed));
        }

        static TestProperty GetTestProperty()
        {
            return TestProperty.Register("XunitTestCase", "xUnit.net Test Case", typeof(string), typeof(VsTestRunner));
        }

        IEnumerable<IGrouping<string, TestCase>> GetTests(IEnumerable<string> sources, IMessageLogger logger, XunitVisualStudioSettings settings, Stopwatch stopwatch)
        {
            var result = new List<IGrouping<string, TestCase>>();

            RemotingUtility.CleanUpRegisteredChannels();

            using (AssemblyHelper.SubscribeResolve())
            {
                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery started", stopwatch.Elapsed));

                foreach (string assemblyFileName in sources)
                {
                    var fileName = Path.GetFileName(assemblyFileName);

                    try
                    {
                        if (cancelled)
                            break;

                        if (!IsXunitTestAssembly(assemblyFileName))
                        {
                            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Skipping: {1}", stopwatch.Elapsed, fileName));
                        }
                        else
                        {
                            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery starting: {1}", stopwatch.Elapsed, fileName));

                            using (var framework = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true))
                            using (var sink = new TestDiscoveryVisitor())
                            {
                                framework.Find(includeSourceInformation: true, messageSink: sink, options: new TestFrameworkOptions());
                                sink.Finished.WaitOne();

                                result.Add(
                                    new Grouping<string, TestCase>(
                                        assemblyFileName,
                                        sink.TestCases
                                            .GroupBy(tc => String.Format("{0}.{1}", tc.Class.Name, tc.Method.Name))
                                            .SelectMany(group => group.Select(testCase => VsDiscoveryVisitor.CreateVsTestCase(assemblyFileName, framework, testCase, settings, forceUniqueNames: group.Count() > 1)))
                                            .ToList()
                                    )
                                );

                                if (settings.MessageDisplay != MessageDisplay.None)
                                    logger.SendMessage(TestMessageLevel.Informational,
                                                       String.Format("[xUnit.net {0}] Discovery finished: {1} ({2} tests)", stopwatch.Elapsed, Path.GetFileName(assemblyFileName), sink.TestCases.Count));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.SendMessage(TestMessageLevel.Error,
                                       String.Format("[xUnit.net {0}] Exception discovering tests from {1}: {2}", stopwatch.Elapsed, assemblyFileName, e));
                    }
                }

                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery complete", stopwatch.Elapsed));

                return result;
            }
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            // Don't try to load ourselves, since we fail (issue #47). Also, Visual Studio Online is brain dead.
            string self = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());
            if (Path.GetFileNameWithoutExtension(assemblyFileName).Equals(self, StringComparison.OrdinalIgnoreCase))
                return false;

            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            string xunitExecutionPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.execution.dll");
            if (!File.Exists(xunitPath) && !File.Exists(xunitExecutionPath))
                return false;

            var assm = Assembly.ReflectionOnlyLoadFrom(assemblyFileName);
            var attrib = assm.GetCustomAttributes(typeof(TargetFrameworkAttribute))
                             .Cast<TargetFrameworkAttribute>()
                             .FirstOrDefault();

            if (attrib != null && attrib.FrameworkName != null)
            {
                // We found the TargetFramework attribute, check for Xamarin
                var xamFound = attrib.FrameworkName.StartsWith("MonoTouch", StringComparison.OrdinalIgnoreCase) ||
                               attrib.FrameworkName.StartsWith("MonoAndroid", StringComparison.OrdinalIgnoreCase);
                return !xamFound;
            }

            return true;
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("sources", sources);

            var stopwatch = Stopwatch.StartNew();
            RunTests(runContext, frameworkHandle, stopwatch, settings => GetTests(sources, frameworkHandle, settings, stopwatch));
            stopwatch.Stop();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("tests", tests);

            var stopwatch = Stopwatch.StartNew();
            RunTests(runContext, frameworkHandle, stopwatch, settings => tests.GroupBy(testCase => testCase.Source));
            stopwatch.Stop();
        }

        void RunTests(IRunContext runContext, IFrameworkHandle frameworkHandle, Stopwatch stopwatch, Func<XunitVisualStudioSettings, IEnumerable<IGrouping<string, TestCase>>> testCaseAccessor)
        {
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            var settings = SettingsProvider.Load();
            var shuttingDown = !runContext.KeepAlive || settings.ShutdownAfterRun;

            if (runContext.KeepAlive && settings.ShutdownAfterRun)
                frameworkHandle.EnableShutdownAfterTestRun = true;

            var toDispose = new List<IDisposable>();

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution started", stopwatch.Elapsed));
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Settings: MaxParallelThreads = {1}, NameDisplay = {2}, ParallelizeAssemblies = {3}, ParallelizeTestCollections = {4}, ShutdownAfterRun = {5}",
                                                                                              stopwatch.Elapsed,
                                                                                              settings.MaxParallelThreads,
                                                                                              settings.NameDisplay,
                                                                                              settings.ParallelizeAssemblies,
                                                                                              settings.ParallelizeTestCollections,
                                                                                              settings.ShutdownAfterRun));
                }

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                    if (settings.ParallelizeAssemblies)
                        testCaseAccessor(settings)
                            .Select(testCaseGroup => RunTestsInAssemblyAsync(runContext, frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup, settings, stopwatch))
                            .ToList()
                            .ForEach(@event => @event.WaitOne());
                    else
                        testCaseAccessor(settings)
                            .ToList()
                            .ForEach(testCaseGroup => RunTestsInAssembly(runContext, frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup, settings, stopwatch));
            }
            finally
            {
                if (!shuttingDown)
                    toDispose.ForEach(disposable => disposable.Dispose());
            }

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution complete", stopwatch.Elapsed));
        }

        void RunTestsInAssembly(IDiscoveryContext discoveryContext,
                                IFrameworkHandle frameworkHandle,
                                List<IDisposable> toDispose,
                                string assemblyFileName,
                                IEnumerable<TestCase> testCases,
                                XunitVisualStudioSettings settings,
                                Stopwatch stopwatch)
        {
            if (cancelled)
                return;

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution starting: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));

            var controller = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true);

            lock (toDispose)
                toDispose.Add(controller);

            var xunitTestCases = testCases.ToDictionary(tc => controller.Deserialize(tc.GetPropertyValue<string>(SerializedTestCaseProperty, null)));

            using (var executionVisitor = new VsExecutionVisitor(discoveryContext, frameworkHandle, xunitTestCases, () => cancelled))
            {
                var executionOptions = new XunitExecutionOptions
                {
                    DisableParallelization = !settings.ParallelizeTestCollections,
                    MaxParallelThreads = settings.MaxParallelThreads
                };

                controller.RunTests(xunitTestCases.Keys.ToList(), executionVisitor, executionOptions);
                executionVisitor.Finished.WaitOne();
            }

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution finished: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));
        }

        ManualResetEvent RunTestsInAssemblyAsync(IDiscoveryContext discoveryContext,
                                                 IFrameworkHandle frameworkHandle,
                                                 List<IDisposable> toDispose,
                                                 string assemblyFileName,
                                                 IEnumerable<TestCase> testCases,
                                                 XunitVisualStudioSettings settings,
                                                 Stopwatch stopwatch)
        {
            var @event = new ManualResetEvent(initialState: false);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    RunTestsInAssembly(discoveryContext, frameworkHandle, toDispose, assemblyFileName, testCases, settings, stopwatch);
                }
                finally
                {
                    @event.Set();
                }
            });

            return @event;
        }

        class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            readonly IEnumerable<TElement> elements;

            public Grouping(TKey key, IEnumerable<TElement> elements)
            {
                Key = key;
                this.elements = elements;
            }

            public TKey Key { get; private set; }

            public IEnumerator<TElement> GetEnumerator()
            {
                return elements.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return elements.GetEnumerator();
            }
        }
    }
}
