using System;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class ResultVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly int totalTests;

        public ResultVisitor(ITestListener listener, int totalTests)
        {
            this.totalTests = totalTests;
            TestListener = listener;
            TestRunState = TestRunState.NoTests;
        }

        public ITestListener TestListener { get; private set; }
        public TestRunState TestRunState { get; set; }

        protected override bool Visit(ITestFailed failed)
        {
            TestRunState = TestRunState.Failure;

            var testResult = failed.ToTdNetTestResult(TestState.Failed, totalTests);

            testResult.Message = ExceptionUtility.CombineMessages(failed);
            testResult.StackTrace = ExceptionUtility.CombineStackTraces(failed);

            TestListener.TestFinished(testResult);

            WriteOutput(failed.Test.DisplayName, failed.Output);

            return true;
        }

        protected override bool Visit(ITestPassed passed)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = passed.ToTdNetTestResult(TestState.Passed, totalTests);

            TestListener.TestFinished(testResult);

            WriteOutput(passed.Test.DisplayName, passed.Output);

            return true;
        }

        protected override bool Visit(ITestSkipped skipped)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = skipped.ToTdNetTestResult(TestState.Ignored, totalTests);

            testResult.Message = skipped.Reason;

            TestListener.TestFinished(testResult);

            return true;
        }

        protected override bool Visit(IErrorMessage errorMessage)
        {
            ReportError("Fatal Error", errorMessage);

            return true;
        }

        protected override bool Visit(ITestAssemblyCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Assembly Cleanup Failure ({0})", cleanupFailure.TestAssembly.Assembly.AssemblyPath), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCaseCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Case Cleanup Failure ({0})", cleanupFailure.TestCase.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestClassCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Class Cleanup Failure ({0})", cleanupFailure.TestClass.Class.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCollectionCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Collection Cleanup Failure ({0})", cleanupFailure.TestCollection.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Cleanup Failure ({0})", cleanupFailure.Test.DisplayName), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        protected override bool Visit(ITestMethodCleanupFailure cleanupFailure)
        {
            ReportError(string.Format("Test Method Cleanup Failure ({0})", cleanupFailure.TestMethod.Method.Name), cleanupFailure);

            return base.Visit(cleanupFailure);
        }

        void ReportError(string messageType, IFailureInformation failureInfo)
        {
            TestRunState = TestRunState.Failure;

            var testResult = new TestResult
            {
                Name = string.Format("*** {0} ***", messageType),
                State = TestState.Failed,
                TimeSpan = TimeSpan.Zero,
                TotalTests = 1,
                Message = ExceptionUtility.CombineMessages(failureInfo),
                StackTrace = ExceptionUtility.CombineStackTraces(failureInfo)
            };

            TestListener.TestFinished(testResult);
        }

        private void WriteOutput(string name, string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            TestListener.WriteLine(string.Format("Output from {0}:", name), Category.Output);
            foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                TestListener.WriteLine(string.Format("  {0}", line), Category.Output);
        }
    }
}