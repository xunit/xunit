using System;
using System.Threading;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class ResultVisitor : TestMessageVisitor2, IDisposable
    {
        readonly int totalTests;

        public ResultVisitor(ITestListener listener, int totalTests)
        {
            this.totalTests = totalTests;
            TestListener = listener;
            TestRunState = TestRunState.NoTests;

            TestFailedEvent += HandleTestFailed;
            TestPassedEvent += HandleTestPassed;
            TestSkippedEvent += HandleTestSkipped;
            ErrorMessageEvent += HandleErrorMessage;
            TestAssemblyCleanupFailureEvent += HandleTestAssemblyCleanupFailure;
            TestCaseCleanupFailureEvent += HandleTestCaseCleanupFailure;
            TestClassCleanupFailureEvent += HandleTestClassCleanupFailure;
            TestCollectionCleanupFailureEvent += HandleTestCollectionCleanupFailure;
            TestMethodCleanupFailureEvent += HandleTestMethodCleanupFailure;
            TestCleanupFailureEvent += HandleTestCleanupFailure;
            TestAssemblyFinishedEvent += HandleTestAssemblyFinished;
        }

        public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);
        public ITestListener TestListener { get; private set; }
        public TestRunState TestRunState { get; set; }

        protected virtual void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var testFailed = args.Message;
            TestRunState = TestRunState.Failure;

            var testResult = testFailed.ToTdNetTestResult(TestState.Failed, totalTests);

            testResult.Message = ExceptionUtility.CombineMessages(testFailed);
            testResult.StackTrace = ExceptionUtility.CombineStackTraces(testFailed);

            TestListener.TestFinished(testResult);

            WriteOutput(testFailed.Test.DisplayName, testFailed.Output);
        }

        protected virtual void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var testPassed = args.Message;
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = testPassed.ToTdNetTestResult(TestState.Passed, totalTests);

            TestListener.TestFinished(testResult);

            WriteOutput(testPassed.Test.DisplayName, testPassed.Output);
        }

        protected virtual void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
        {
            var testSkipped = args.Message;
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = testSkipped.ToTdNetTestResult(TestState.Ignored, totalTests);

            testResult.Message = testSkipped.Reason;

            TestListener.TestFinished(testResult);
        }

        protected virtual void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
        {
            var error = args.Message;
            ReportError("Fatal Error", error);
        }

        protected virtual void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Assembly Cleanup Failure ({cleanupFailure.TestAssembly.Assembly.AssemblyPath})", cleanupFailure);
        }

        protected virtual void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Case Cleanup Failure ({cleanupFailure.TestCase.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Class Cleanup Failure ({cleanupFailure.TestClass.Class.Name})", cleanupFailure);
        }

        protected virtual void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Collection Cleanup Failure ({cleanupFailure.TestCollection.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Cleanup Failure ({cleanupFailure.Test.DisplayName})", cleanupFailure);
        }

        protected virtual void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
        {
            var cleanupFailure = args.Message;
            ReportError($"Test Method Cleanup Failure ({cleanupFailure.TestMethod.Method.Name})", cleanupFailure);
        }

        protected virtual void HandleTestAssemblyFinished(MessageHandlerArgs<ITestAssemblyFinished> args)
        {
            Finished.Set();
        }

        void ReportError(string messageType, IFailureInformation failureInfo)
        {
            TestRunState = TestRunState.Failure;

            var testResult = new TestResult
            {
                Name = $"*** {messageType} ***",
                State = TestState.Failed,
                TimeSpan = TimeSpan.Zero,
                TotalTests = 1,
                Message = ExceptionUtility.CombineMessages(failureInfo),
                StackTrace = ExceptionUtility.CombineStackTraces(failureInfo)
            };

            TestListener.TestFinished(testResult);
        }

        void WriteOutput(string name, string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            TestListener.WriteLine($"Output from {name}:", Category.Output);
            foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                TestListener.WriteLine($"  {line}", Category.Output);
        }

        public void Dispose()
        {
            ((IDisposable)Finished).Dispose();
        }
    }
}