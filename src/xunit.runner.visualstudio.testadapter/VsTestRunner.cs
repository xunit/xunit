using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Abstractions;
using Xunit.Runner.VisualStudio.Settings;

namespace Xunit.Runner.VisualStudio.TestAdapter
{



    [FileExtension(".appx")]
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    [ExtensionUri(Constants.ExecutorUri)]
    public class VsTestRunner : ITestDiscoverer, ITestExecutor
    {
        public static TestProperty SerializedTestCaseProperty = GetTestProperty();

        private static HashSet<string> platformAssemblies = new HashSet<string>
            (
            new [] {"MICROSOFT.VISUALSTUDIO.TESTPLATFORM.UNITTESTFRAMEWORK.DLL", 
                    "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.CORE.DLL", 
                    "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.TESTEXECUTOR.CORE.DLL", 
                    "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.EXTENSIONS.MSAPPCONTAINERADAPTER.DLL", 
                    "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.OBJECTMODEL.DLL", 
                    "MICROSOFT.VISUALSTUDIO.TESTPLATFORM.UTILITIES.DLL", 
                    "VSTEST.EXECUTIONENGINE.APPCONTAINER.EXE", 
                    "VSTEST.EXECUTIONENGINE.APPCONTAINER.X86.EXE",
                    "XUNIT.EXECUTION.DLL",
                    "XUNIT.RUNNER.UTILITY.DLL",
                    "XUNIT.RUNNER.VISUALSTUDIO.TESTADAPTER.DLL",
                    "XUNIT.CORE.DLL",
                    "XUNIT.ASSERT.DLL"}
            );

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
            Guard.ArgumentValid("sources", "appx not supported for discovery", !ContainsAppX(sources));

            DiscoverTests(
                sources,
                logger,
                SettingsProvider.Load(),
                (source, discoverer) => new VsDiscoveryVisitor(source, discoverer, logger, discoveryContext, discoverySink, () => cancelled)
            );
        }

        void DiscoverTests<TVisitor>(IEnumerable<string> sources,
                                     IMessageLogger logger,
                                     XunitVisualStudioSettings settings,
                                     Func<string, ITestFrameworkDiscoverer, TVisitor> visitorFactory,
                                     Action<string, ITestFrameworkDiscoverer, TVisitor> visitComplete = null,
                                     Stopwatch stopwatch = null)
            where TVisitor : IVsDiscoveryVisitor
        {
            if (stopwatch == null)
                stopwatch = Stopwatch.StartNew();

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
                                    logger.SendMessage(TestMessageLevel.Informational,
                                                       String.Format("[xUnit.net {0}] Skipping: {1} (no reference to xUnit.net)", stopwatch.Elapsed, fileName));
                            }
                            else
                            {
                                using (var framework = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true))
                                using (var visitor = visitorFactory(assemblyFileName, framework))
                                {
                                    var targetFramework = framework.TargetFramework;
                                    if (targetFramework.StartsWith("MonoTouch", StringComparison.OrdinalIgnoreCase) ||
                                        targetFramework.StartsWith("MonoAndroid", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                            logger.SendMessage(TestMessageLevel.Informational, String.Format("[xUnit.net {0}] Skipping: {1} (unsupported target framework '{2}')", stopwatch.Elapsed, fileName, targetFramework));
                                    }
                                    else
                                    {
                                        if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                            logger.SendMessage(TestMessageLevel.Informational,
                                                               String.Format("[xUnit.net {0}] Discovery starting: {1}", stopwatch.Elapsed, fileName));

                                        framework.Find(includeSourceInformation: true, messageSink: visitor, options: new TestFrameworkOptions());
                                        var totalTests = visitor.Finish();

                                        if (visitComplete != null)
                                            visitComplete(assemblyFileName, framework, visitor);

                                        if (settings.MessageDisplay == MessageDisplay.Diagnostic)
                                            logger.SendMessage(TestMessageLevel.Informational,
                                                               String.Format("[xUnit.net {0}] Discovery finished: {1} ({2} tests)", stopwatch.Elapsed, fileName, totalTests));
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            var ex = e.Unwrap();
                            var fileNotFound = ex as FileNotFoundException;
#if !WINDOWS_PHONE_APP
                            var fileLoad = ex as FileLoadException;
#endif
                            if (fileNotFound != null)
                                logger.SendMessage(TestMessageLevel.Informational,
                                                   String.Format("[xUnit.net {0}] Skipping: {1} (could not find dependent assembly '{2}')", stopwatch.Elapsed, fileName, Path.GetFileNameWithoutExtension(fileNotFound.FileName)));
#if !WINDOWS_PHONE_APP
                            else if (fileLoad != null)
                                logger.SendMessage(TestMessageLevel.Informational,
                                                   String.Format("[xUnit.net {0}] Skipping: {1} (could not find dependent assembly '{2}')", stopwatch.Elapsed, fileName, Path.GetFileNameWithoutExtension(fileLoad.FileName)));
#endif
                            else
                                logger.SendMessage(TestMessageLevel.Error,
                                                   String.Format("[xUnit.net {0}] Exception discovering tests from {1}: {2}", stopwatch.Elapsed, fileName, ex));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.SendMessage(TestMessageLevel.Error,
                                   String.Format("[xUnit.net {0}] Exception discovering tests: {1}", stopwatch.Elapsed, e.Unwrap()));
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
#if WIN8_STORE
            // For store apps, the files are copied to the AppX dir, we need to load it from there
            sources = sources.Select(s => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                                                     .Location), Path.GetFileName(s)));
            
#elif WINDOWS_PHONE_APP
            sources = sources.Select(s => Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, Path.GetFileName(s))); ;
#endif
            
            var result = new List<IGrouping<string, TestCase>>();

            DiscoverTests(
                sources,
                logger,
                settings,
                (source, discoverer) => new VsExecutionDiscoveryVisitor(),
                (source, discoverer, visitor) =>
                    result.Add(
                        new Grouping<string, TestCase>(
                            source,
                            visitor.TestCases
                                   .GroupBy(tc => String.Format("{0}.{1}", tc.TestMethod.TestClass.Class.Name, tc.TestMethod.Method.Name))
                                   .SelectMany(group => group.Select(testCase => VsDiscoveryVisitor.CreateVsTestCase(source, discoverer, testCase, settings, forceUniqueNames: group.Count() > 1)))
                                   .ToList()
                            )
                        ),
                stopwatch
            );
            
            return result;
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            // Don't try to load ourselves, since we fail (issue #47). Also, Visual Studio Online is brain dead.
            // or any test framework assembly
            if (platformAssemblies.Contains(Path.GetFileName(assemblyFileName)
                                                .ToUpperInvariant()))
                return false;

            
            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            string xunitExecutionPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.execution.dll");
            
            return File.Exists(xunitPath) || File.Exists(xunitExecutionPath);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("sources", sources);

            
            // In this case, we need to go thru the files manually
            if (ContainsAppX(sources))
            {
#if !WINDOWS_PHONE_APP
                var files = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll", SearchOption.TopDirectoryOnly);
#else
                var files = Directory.GetFiles(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "*.dll");
#endif

                var toAdd = from file in files
                            let system = platformAssemblies.Contains(Path.GetFileName(file)
                                                                         .ToUpperInvariant())
                            where !system
                            select file;

                sources = toAdd.ToList();
            }

            var stopwatch = Stopwatch.StartNew();
            RunTests(runContext, frameworkHandle, stopwatch, settings => GetTests(sources, frameworkHandle, settings, stopwatch));
            stopwatch.Stop();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("tests", tests);
            Guard.ArgumentValid("tests", "appx not supported in this overload", !ContainsAppX(tests.Select(t => t.Source)));

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

#if WIN8_STORE 
            // For store apps, the files are copied to the AppX dir, we need to load it from there
            assemblyFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                    .Location), Path.GetFileName(assemblyFileName));
#elif WINDOWS_PHONE_APP

            // For WPA Apps, use the package location
            assemblyFileName = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, Path.GetFileName(assemblyFileName));
#endif

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

            Task.Run(() =>
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


        private static bool ContainsAppX(IEnumerable<string> sources)
        {
            return sources.Any(s => string.Compare(Path.GetExtension(s), ".appx", StringComparison.OrdinalIgnoreCase) == 0);
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
