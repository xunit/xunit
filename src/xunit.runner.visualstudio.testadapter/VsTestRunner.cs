using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var sourceSinks = new List<SourceSink<VsDiscoveryVisitor>>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery started", stopwatch.Elapsed));

                using (AssemblyHelper.SubscribeResolve())
                {
                    try
                    {
                        foreach (string assemblyFileName in sources)
                        {
                            try
                            {
                                if (cancelled)
                                    break;

                                if (!IsXunitTestAssembly(assemblyFileName))
                                {
                                    if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                        logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Skipping: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));
                                }
                                else
                                {
                                    if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                        logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery starting: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));

                                    var framework = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true);
                                    var sink = new VsDiscoveryVisitor(assemblyFileName, framework, logger, discoverySink, () => cancelled);
                                    sourceSinks.Add(new SourceSink<VsDiscoveryVisitor> { Framework = framework, Sink = sink, AssemblyFileName = assemblyFileName });
                                    framework.Find(includeSourceInformation: true, messageSink: sink);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.SendMessage(TestMessageLevel.Error,
                                                   String.Format("[xUnit.net {0}] Exception discovering tests from {1}: {2}", stopwatch.Elapsed, assemblyFileName, e));
                            }
                        }

                        var toFinish = new List<SourceSink<VsDiscoveryVisitor>>(sourceSinks);

                        while (toFinish.Count > 0)
                        {
                            var finishedIdx = WaitHandle.WaitAny(sourceSinks.Select(sink => sink.Sink.Finished).ToArray());
                            var sourceSink = toFinish[finishedIdx];

                            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                logger.SendMessage(TestMessageLevel.Informational,
                                                   String.Format("[xUnit.net {0}] Discovery finished: {1} ({2} tests)", stopwatch.Elapsed, Path.GetFileName(sourceSink.AssemblyFileName), sourceSink.Sink.TotalTests));

                            toFinish.RemoveAt(finishedIdx);
                        }
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
            var sourceSinks = new List<SourceSink<TestDiscoveryVisitor>>();
            var result = new List<IGrouping<string, TestCase>>();

            try
            {
                if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                    lock (stopwatch)
                        logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery started", stopwatch.Elapsed));

                foreach (string assemblyFileName in sources)
                {
                    try
                    {
                        if (cancelled)
                            break;

                        if (!IsXunitTestAssembly(assemblyFileName))
                        {
                            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                lock (stopwatch)
                                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Skipping: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));
                        }
                        else
                        {
                            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                lock (stopwatch)
                                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery starting: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));

                            var framework = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true);
                            var sink = new TestDiscoveryVisitor();
                            sourceSinks.Add(new SourceSink<TestDiscoveryVisitor> { Framework = framework, Sink = sink, AssemblyFileName = assemblyFileName });
                            framework.Find(includeSourceInformation: true, messageSink: sink);
                        }
                    }
                    catch (Exception e)
                    {
                        lock (stopwatch)
                            logger.SendMessage(TestMessageLevel.Error,
                                           String.Format("[xUnit.net {0}] Exception discovering tests from {1}: {2}", stopwatch.Elapsed, assemblyFileName, e));
                    }
                }

                var toFinish = new List<SourceSink<TestDiscoveryVisitor>>(sourceSinks);

                while (toFinish.Count > 0)
                {
                    var finishedIdx = WaitHandle.WaitAny(sourceSinks.Select(sink => sink.Sink.Finished).ToArray());
                    var sourceSink = toFinish[finishedIdx];

                    result.Add(
                        new Grouping<string, TestCase>(
                            sourceSink.AssemblyFileName,
                            sourceSink.Sink.TestCases
                                           .GroupBy(tc => String.Format("{0}.{1}", tc.Class.Name, tc.Method.Name))
                                           .SelectMany(group => group.Select(testCase => VsDiscoveryVisitor.CreateVsTestCase(sourceSink.AssemblyFileName, sourceSink.Framework, testCase, settings, forceUniqueNames: group.Count() > 1)))
                                           .ToList()
                        )
                    );

                    if (settings.MessageDisplay != MessageDisplay.None)
                        lock (stopwatch)
                            logger.SendMessage(TestMessageLevel.Informational,
                                           String.Format("[xUnit.net {0}] Discovery finished: {1} ({2} tests)", stopwatch.Elapsed, Path.GetFileName(sourceSink.AssemblyFileName), sourceSink.Sink.TestCases.Count));

                    toFinish.RemoveAt(finishedIdx);
                }
            }
            finally
            {
                foreach (var sourceSink in sourceSinks)
                {
                    sourceSink.Sink.Dispose();
                    sourceSink.Framework.Dispose();
                }
            }

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Discovery complete", stopwatch.Elapsed));

            return result;
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
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution started", stopwatch.Elapsed));

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                using (AssemblyHelper.SubscribeResolve())
                    if (settings.ParallelizeAssemblies)
                        testCaseAccessor(settings)
                            .Select(testCaseGroup => RunTestsInAssemblyAsync(frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup, settings, stopwatch))
                            .ToList()
                            .ForEach(@event => @event.WaitOne());
                    else
                        testCaseAccessor(settings)
                            .ToList()
                            .ForEach(testCaseGroup => RunTestsInAssembly(frameworkHandle, toDispose, testCaseGroup.Key, testCaseGroup, settings, stopwatch));
            }
            finally
            {
                if (!shuttingDown)
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Thread.Sleep(5000);  // Try to prevent erroneous "unloaded app domain" errors in Visual Studio
                        toDispose.ForEach(disposable => disposable.Dispose());
                    });
            }

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution complete", stopwatch.Elapsed));
        }

        void RunTestsInAssembly(IFrameworkHandle frameworkHandle,
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

            using (var executionVisitor = new VsExecutionVisitor(frameworkHandle, xunitTestCases, () => cancelled))
            {
                controller.Run(xunitTestCases.Keys.ToList(), executionVisitor);
                executionVisitor.Finished.WaitOne();
            }

            if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                lock (stopwatch)
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Execution finished: {1}", stopwatch.Elapsed, Path.GetFileName(assemblyFileName)));
        }

        ManualResetEvent RunTestsInAssemblyAsync(IFrameworkHandle frameworkHandle,
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
                    RunTestsInAssembly(frameworkHandle, toDispose, assemblyFileName, testCases, settings, stopwatch);
                }
                finally
                {
                    @event.Set();
                }
            });

            return @event;
        }

        class SourceSink<TSink> where TSink : IDisposable
        {
            public string AssemblyFileName;
            public XunitFrontController Framework;
            public TSink Sink;
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