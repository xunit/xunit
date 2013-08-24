using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Sdk;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Xunit.Runner.VisualStudio
{
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(VsTestRunner.ExecutorUri)]
    [ExtensionUri(VsTestRunner.ExecutorUri)]
    public class VsTestRunner : ITestDiscoverer, ITestExecutor
    {
        public const string ExecutorUri = "executor://xunit.codeplex.com/VsTestRunner";

        static Action<TestCase, string, string> addTraitThunk = GetAddTraitThunk();
        static Uri uri = new Uri(ExecutorUri);

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

            RemotingUtility.CleanUpRegisteredChannels();

            foreach (string source in sources)
                try
                {
                    if (IsXunitTestAssembly(source))
                        using (ExecutorWrapper executor = new ExecutorWrapper(source, configFilename: null, shadowCopy: true))
                            foreach (TestCase testCase in GetTestCases(executor))
                                discoverySink.SendTestCase(testCase);
                }
                catch (Exception e)
                {
                    logger.SendMessage(TestMessageLevel.Error, String.Format("xUnit.net: Exception discovering tests from {0}: {1}", source, e));
                }
        }

        static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {
            return displayName == fullyQualifiedMethodName ? shortMethodName : displayName;
        }

        static Action<TestCase, string, string> GetAddTraitThunk()
        {
            try
            {
                Type testCaseType = typeof(TestCase);
                Type stringType = typeof(string);
                PropertyInfo property = testCaseType.GetProperty("Traits");

                if (property == null)
                    return null;

                MethodInfo method = property.PropertyType.GetMethod("Add", new[] { typeof(string), typeof(string) });
                if (method == null)
                    return null;

                var thisParam = Expression.Parameter(testCaseType, "this");
                var nameParam = Expression.Parameter(stringType, "name");
                var valueParam = Expression.Parameter(stringType, "value");
                var instance = Expression.Property(thisParam, property);
                var body = Expression.Call(instance, method, new[] { nameParam, valueParam });

                return Expression.Lambda<Action<TestCase, string, string>>(body, thisParam, nameParam, valueParam).Compile();
            }
            catch (Exception)
            {
                return null;
            }
        }

        static TestCase GetTestCase(DiaSessionWrapper diaSession, string source, XmlNode methodNode)
        {
            string typeName = methodNode.Attributes["type"].Value;
            string methodName = methodNode.Attributes["method"].Value;
            string displayName = methodNode.Attributes["name"].Value;
            string fullyQualifiedName = String.Format("{0}.{1}", typeName, methodName);

            TestCase testCase = new TestCase(fullyQualifiedName, uri, source)
            {
                DisplayName = GetDisplayName(displayName, methodName, fullyQualifiedName),
            };

            if (addTraitThunk != null)
                foreach (XmlNode traitNode in methodNode.SelectNodes("traits/trait"))
                {
                    string value = traitNode.Attributes["name"].Value;
                    string value2 = traitNode.Attributes["value"].Value;
                    addTraitThunk(testCase, value, value2);
                }

            DiaNavigationData navigationData = diaSession.GetNavigationData(typeName, methodName);
            if (navigationData != null)
            {
                testCase.CodeFilePath = navigationData.FileName;
                testCase.LineNumber = navigationData.MinLineNumber;
            }

            return testCase;
        }

        static IEnumerable<TestCase> GetTestCases(ExecutorWrapper executor)
        {
            string source = executor.AssemblyFilename;

            using (DiaSessionWrapper diaSession = new DiaSessionWrapper(source))
                foreach (XmlNode methodNode in executor.EnumerateTests().SelectNodes("//method"))
                    yield return GetTestCase(diaSession, source, methodNode);
        }

        static bool IsXunitTestAssembly(string assemblyFileName)
        {
            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            return File.Exists(xunitPath);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            frameworkHandle.EnableShutdownAfterTestRun = true;

            var cleanupList = new List<ExecutorWrapper>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                foreach (string source in sources)
                    if (VsTestRunner.IsXunitTestAssembly(source))
                        RunTestsInAssembly(cleanupList, source, frameworkHandle);
            }
            finally
            {
                Thread.Sleep(1000);

                foreach (var executorWrapper in cleanupList)
                    executorWrapper.Dispose();
            }
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            Guard.ArgumentNotNull("tests", tests);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("frameworkHandle", frameworkHandle);

            frameworkHandle.EnableShutdownAfterTestRun = true;

            var cleanupList = new List<ExecutorWrapper>();

            try
            {
                RemotingUtility.CleanUpRegisteredChannels();

                cancelled = false;

                foreach (var testCaseGroup in tests.GroupBy(tc => tc.Source))
                    if (VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                        RunTestsInAssembly(cleanupList, testCaseGroup.Key, frameworkHandle, testCaseGroup);
            }
            finally
            {
                Thread.Sleep(1000);

                foreach (var executorWrapper in cleanupList)
                    executorWrapper.Dispose();
            }
        }

        void RunTestsInAssembly(List<ExecutorWrapper> cleanupList, string assemblyFileName, ITestExecutionRecorder recorder, IEnumerable<TestCase> testCases = null)
        {
            if (cancelled)
                return;

            var executor = new ExecutorWrapper(assemblyFileName, configFilename: null, shadowCopy: true);
            cleanupList.Add(executor);

            if (testCases == null)
                testCases = VsTestRunner.GetTestCases(executor).ToArray();

            var logger = new VsRunnerLogger(recorder, testCases, () => cancelled);
            var runner = new TestRunner(executor, logger);

            foreach (var testClass in testCases.Select(tc => new TypeAndMethod(tc.FullyQualifiedName))
                                               .GroupBy(tam => tam.Type))
            {
                runner.RunTests(testClass.Key, testClass.Select(tam => tam.Method).ToList());
                if (cancelled)
                    return;
            }
        }

        class VsRunnerLogger : IRunnerLogger
        {
            Func<bool> cancelledThunk;
            ITestExecutionRecorder recorder;
            Dictionary<string, TestCase> testCases;

            public VsRunnerLogger(ITestExecutionRecorder recorder, IEnumerable<TestCase> testCases, Func<bool> cancelledThunk)
            {
                this.recorder = recorder;
                this.testCases = testCases.ToDictionary(tc => tc.FullyQualifiedName);
                this.cancelledThunk = cancelledThunk;
            }

            public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time)
            {
            }

            public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion)
            {
            }

            public bool ClassFailed(string className, string exceptionType, string message, string stackTrace)
            {
                recorder.SendMessage(TestMessageLevel.Error, String.Format("Fixture {0} failed: {1}: {2}\r\n{3}", className, exceptionType, message, stackTrace));
                return !cancelledThunk();
            }

            public void ExceptionThrown(string assemblyFilename, Exception exception)
            {
                recorder.SendMessage(TestMessageLevel.Error, String.Format("Catastrophic failure: {0}", exception));
            }

            public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace)
            {
                VsTestResult result = MakeVsTestResult(name, type, method, duration, output, TestOutcome.Failed);
                result.ErrorMessage = message;
                result.ErrorStackTrace = stackTrace;

                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public bool TestFinished(string name, string type, string method)
            {
                return !cancelledThunk();
            }

            public void TestPassed(string name, string type, string method, double duration, string output)
            {
                VsTestResult result = MakeVsTestResult(name, type, method, duration, output, TestOutcome.Passed);
                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public void TestSkipped(string name, string type, string method, string reason)
            {
                VsTestResult result = MakeVsTestResult(name, type, method, 0.0, null, TestOutcome.Skipped);
                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public bool TestStart(string name, string type, string method)
            {
                recorder.RecordStart(GetTestCase(type, method));
                return !cancelledThunk();
            }

            private static string GetFullyQualifiedName(string type, string method)
            {
                return String.Format("{0}.{1}", type, method);
            }

            private TestCase GetTestCase(string type, string method)
            {
                return GetTestCase(GetFullyQualifiedName(type, method));
            }

            private TestCase GetTestCase(string fullyQualifiedName)
            {
                return testCases[fullyQualifiedName];
            }

            private string GetTestResultDisplayName(string testCaseDisplayName, string testResultDisplayName, string fullyQualifiedName)
            {
                // If the display name looks like fully qualified name + parameters (as in the case of
                // [Theory]), we want to follow the same DisplayName pattern we used earlier with the
                // test case.
                if (!testResultDisplayName.StartsWith(fullyQualifiedName, StringComparison.OrdinalIgnoreCase))
                    return testResultDisplayName;

                return testCaseDisplayName + testResultDisplayName.Substring(fullyQualifiedName.Length);
            }

            private VsTestResult MakeVsTestResult(string name, string type, string method, double duration, string output, TestOutcome outcome)
            {
                string fullyQualifiedName = GetFullyQualifiedName(type, method);
                TestCase testCase = GetTestCase(fullyQualifiedName);

                VsTestResult result = new VsTestResult(testCase)
                {
                    ComputerName = Environment.MachineName,
                    DisplayName = GetTestResultDisplayName(testCase.DisplayName, name, fullyQualifiedName),
                    Duration = TimeSpan.FromSeconds(duration),
                    Outcome = outcome,
                };

                // Work around VS considering a test "not run" when the duration is 0
                if (result.Duration.TotalMilliseconds == 0)
                    result.Duration = TimeSpan.FromMilliseconds(1);

                if (!String.IsNullOrEmpty(output))
                    result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));

                return result;
            }
        }

        // Splits a fully qualified method name into the type and method components
        class TypeAndMethod
        {
            public TypeAndMethod(string typeAndMethod)
            {
                int idx = typeAndMethod.LastIndexOf('.');
                Type = typeAndMethod.Substring(0, idx);
                Method = typeAndMethod.Substring(idx + 1);
            }

            public string Method { get; private set; }
            public string Type { get; private set; }
        }
    }
}
