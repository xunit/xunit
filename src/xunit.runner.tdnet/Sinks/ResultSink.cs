using System;
using System.Globalization;
using System.Threading;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class ResultSink : TestMessageSink
    {
        readonly int totalTests;

        public ResultSink(ITestListener listener, int totalTests)
        {
            this.totalTests = totalTests;
            TestListener = listener;
            TestRunState = TestRunState.NoTests;

            Execution.TestFailedEvent += HandleTestFailed;
            Execution.TestPassedEvent += HandleTestPassed;
            Execution.TestSkippedEvent += HandleTestSkipped;

            Diagnostics.ErrorMessageEvent += args => ReportError(args.Message, "Fatal Error");
            Execution.TestAssemblyCleanupFailureEvent += args => ReportError(args.Message, "Test Assembly Cleanup Failure ({0})", args.Message.TestAssembly.Assembly.AssemblyPath);
            Execution.TestCaseCleanupFailureEvent += args => ReportError(args.Message, "Test Case Cleanup Failure ({0})", args.Message.TestCase.DisplayName);
            Execution.TestClassCleanupFailureEvent += args => ReportError(args.Message, "Test Class Cleanup Failure ({0})", args.Message.TestClass.Class.Name);
            Execution.TestCollectionCleanupFailureEvent += args => ReportError(args.Message, "Test Collection Cleanup Failure ({0})", args.Message.TestCollection.DisplayName);
            Execution.TestMethodCleanupFailureEvent += args => ReportError(args.Message, "Test Method Cleanup Failure ({0})", args.Message.TestMethod.Method.Name);
            Execution.TestCleanupFailureEvent += args => ReportError(args.Message, "Test Cleanup Failure ({0})", args.Message.Test.DisplayName);

            Execution.TestAssemblyFinishedEvent += args => Finished.Set();
        }

        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);
        public ITestListener TestListener { get; private set; }
        public TestRunState TestRunState { get; set; }

        void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            TestRunState = TestRunState.Failure;

            var testFailed = args.Message;
            var testResult = testFailed.ToTdNetTestResult(TestState.Failed, totalTests);
            testResult.Message = ExceptionUtility.CombineMessages(testFailed);
            testResult.StackTrace = ExceptionUtility.CombineStackTraces(testFailed);

            TestListener.TestFinished(testResult);

            WriteOutput(testFailed.Test.DisplayName, testFailed.Output);
        }

        void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testPassed = args.Message;
            var testResult = testPassed.ToTdNetTestResult(TestState.Passed, totalTests);

            TestListener.TestFinished(testResult);

            WriteOutput(testPassed.Test.DisplayName, testPassed.Output);
        }

        void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testSkipped = args.Message;
            var testResult = testSkipped.ToTdNetTestResult(TestState.Ignored, totalTests);
            testResult.Message = testSkipped.Reason;

            TestListener.TestFinished(testResult);
        }

        void ReportError(IFailureInformation failureInfo, string messageType)
        {
            TestRunState = TestRunState.Failure;

            var testResult = new TestResult
            {
                Name = string.Format(CultureInfo.CurrentCulture, "*** {0} ***", messageType),
                State = TestState.Failed,
                TimeSpan = TimeSpan.Zero,
                TotalTests = 1,
                Message = ExceptionUtility.CombineMessages(failureInfo),
                StackTrace = ExceptionUtility.CombineStackTraces(failureInfo)
            };

            TestListener.TestFinished(testResult);
        }

        void ReportError(IFailureInformation failureInfo, string messageTypeFormat, params object[] args) =>
            ReportError(failureInfo, string.Format(CultureInfo.CurrentCulture, messageTypeFormat, args));

        void WriteOutput(string name, string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            TestListener.WriteLine(string.Format(CultureInfo.CurrentCulture, "Output from {0}:", name), Category.Output);
            foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                TestListener.WriteLine(string.Format(CultureInfo.CurrentCulture, "  {0}", line), Category.Output);
        }

        public override void Dispose()
        {
            base.Dispose();
            Finished.Dispose();
        }
    }
}
