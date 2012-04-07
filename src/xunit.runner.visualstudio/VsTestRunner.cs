using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Sdk;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Xunit.Runner.VisualStudio {
    [FileExtension(".dll")]
    [FileExtension(".exe")]
    [DefaultExecutorUri(VsTestRunner.ExecutorUri)]
    [ExtensionUri(VsTestRunner.ExecutorUri)]
    public class VsTestRunner : ITestDiscoverer, ITestExecutor {
        public const string ExecutorUri = "executor://xunit.codeplex.com/VsTestRunner";

        private static Uri uri = new Uri(ExecutorUri);

        private bool cancelled;

        public void Cancel() {
            cancelled = true;
        }

        public void DiscoverTests(IEnumerable<string> sources, IMessageLogger logger, ITestCaseDiscoverySink discoverySink) {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("logger", logger);
            Guard.ArgumentNotNull("discoverySink", discoverySink);

            foreach (string source in sources)
                try {
                    if (IsXunitTestAssembly(source))
                        using (ExecutorWrapper executor = new ExecutorWrapper(source, configFilename: null, shadowCopy: true))
                            foreach (TestCase testCase in GetTestCases(executor))
                                discoverySink.SendTestCase(testCase);
                }
                catch (Exception e) {
                    logger.SendMessage(TestMessageLevel.Error, String.Format("xUnit.net: Exception discovering tests from {0}: {1}", source, e));
                }
        }

        static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName) {
            return displayName == fullyQualifiedMethodName ? shortMethodName : displayName;
        }

        static TestCase GetTestCase(string source, XmlNode methodNode) {
            string typeName = methodNode.Attributes["type"].Value;
            string methodName = methodNode.Attributes["method"].Value;
            string displayName = methodNode.Attributes["name"].Value;
            string fullyQualifiedName = typeName + "." + methodName;

            TestCase testCase = new TestCase(fullyQualifiedName, uri) {
                DisplayName = GetDisplayName(displayName, methodName, fullyQualifiedName),
                Source = source,
            };

            try {
                using (DiaSession diaSession = new DiaSession(source)) {
                    DiaNavigationData navigationData = diaSession.GetNavigationData(typeName, methodName);
                    testCase.CodeFilePath = navigationData.FileName;
                    testCase.LineNumber = navigationData.MinLineNumber;
                }
            }
            catch { } // DiaSession throws if the PDB file is missing or corrupt

            return testCase;
        }

        static IEnumerable<TestCase> GetTestCases(ExecutorWrapper executor) {
            foreach (XmlNode methodNode in executor.EnumerateTests().SelectNodes("//method"))
                yield return GetTestCase(executor.AssemblyFilename, methodNode);
        }

        static bool IsXunitTestAssembly(string assemblyFileName) {
            string xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            return File.Exists(xunitPath);
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, ITestExecutionRecorder testExecutionRecorder) {
            Guard.ArgumentNotNull("sources", sources);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("testExecutionRecorder", testExecutionRecorder);

            foreach (string source in sources)
                if (VsTestRunner.IsXunitTestAssembly(source))
                    RunTestsInAssembly(source, runContext, testExecutionRecorder);
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, ITestExecutionRecorder testExecutionRecorder) {
            Guard.ArgumentNotNull("tests", tests);
            Guard.ArgumentNotNull("runContext", runContext);
            Guard.ArgumentNotNull("testExecutionRecorder", testExecutionRecorder);

            foreach (var testCaseGroup in tests.GroupBy(tc => tc.Source))
                if (VsTestRunner.IsXunitTestAssembly(testCaseGroup.Key))
                    RunTestsInAssembly(testCaseGroup.Key, runContext, testExecutionRecorder, testCaseGroup);
        }

        void RunTestsInAssembly(string assemblyFileName, IRunContext ctxt, ITestExecutionRecorder recorder, IEnumerable<TestCase> testCases = null) {
            cancelled = false;

            using (var executor = new ExecutorWrapper(assemblyFileName, configFilename: null, shadowCopy: true)) {
                if (testCases == null)
                    testCases = VsTestRunner.GetTestCases(executor).ToArray();

                var logger = new VsRunnerLogger(recorder, testCases, () => cancelled);
                var runner = new TestRunner(executor, logger);

                foreach (var testClass in testCases.Select(tc => new TypeAndMethod(tc.Name))
                                                   .GroupBy(tam => tam.Type))
                    runner.RunTests(testClass.Key, testClass.Select(tam => tam.Method).ToList());
            }
        }

        class VsRunnerLogger : IRunnerLogger {
            Func<bool> cancelledThunk;
            ITestExecutionRecorder recorder;
            Dictionary<string, TestCase> testCases;

            public VsRunnerLogger(ITestExecutionRecorder recorder, IEnumerable<TestCase> testCases, Func<bool> cancelledThunk) {
                this.recorder = recorder;
                this.testCases = testCases.ToDictionary(tc => tc.Name);
                this.cancelledThunk = cancelledThunk;
            }

            public void AssemblyFinished(string assemblyFilename, int total, int failed, int skipped, double time) {
            }

            public void AssemblyStart(string assemblyFilename, string configFilename, string xUnitVersion) {
            }

            public bool ClassFailed(string className, string exceptionType, string message, string stackTrace) {
                recorder.SendMessage(TestMessageLevel.Error, String.Format("Fixture {0} failed: {1}: {2}\r\n{3}", className, exceptionType, message, stackTrace));
                return !cancelledThunk();
            }

            public void ExceptionThrown(string assemblyFilename, Exception exception) {
                recorder.SendMessage(TestMessageLevel.Error, String.Format("Catastrophic failure: {0}", exception));
            }

            public void TestFailed(string name, string type, string method, double duration, string output, string exceptionType, string message, string stackTrace) {
                VsTestResult result = MakeVsTestResult(type, method, duration, output, TestOutcome.Failed);
                result.ErrorMessage = message;
                result.ErrorStackTrace = stackTrace;

                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public bool TestFinished(string name, string type, string method) {
                return !cancelledThunk();
            }

            public void TestPassed(string name, string type, string method, double duration, string output) {
                VsTestResult result = MakeVsTestResult(type, method, duration, output, TestOutcome.Passed);
                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public void TestSkipped(string name, string type, string method, string reason) {
                VsTestResult result = MakeVsTestResult(type, method, 0.0, null, TestOutcome.Skipped);
                recorder.RecordEnd(result.TestCase, result.Outcome);
                recorder.RecordResult(result);
            }

            public bool TestStart(string name, string type, string method) {
                recorder.RecordStart(GetTestCase(type, method));
                return !cancelledThunk();
            }

            private TestCase GetTestCase(string type, string method) {
                string fullyQualifiedName = type + "." + method;
                return testCases[fullyQualifiedName];
            }

            private VsTestResult MakeVsTestResult(string type, string method, double duration, string output, TestOutcome outcome) {
                VsTestResult result = new VsTestResult(GetTestCase(type, method)) {
                    Duration = TimeSpan.FromSeconds(duration),
                    ComputerName = Environment.MachineName,
                    Outcome = outcome,
                };

                if (!String.IsNullOrWhiteSpace(output))
                    result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, output));

                return result;
            }
        }

        // Splits a fully qualified method name into the type and method components
        class TypeAndMethod {
            public TypeAndMethod(string typeAndMethod) {
                int idx = typeAndMethod.LastIndexOf('.');
                Type = typeAndMethod.Substring(0, idx);
                Method = typeAndMethod.Substring(idx + 1);
            }

            public string Method { get; private set; }
            public string Type { get; private set; }
        }
    }
}
