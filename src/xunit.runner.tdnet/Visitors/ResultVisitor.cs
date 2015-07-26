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

        protected override bool Visit(ITestFailed testFailed)
        {
            TestRunState = TestRunState.Failure;

            var testResult = testFailed.ToTdNetTestResult(TestState.Failed, totalTests);

            testResult.Message = ExceptionUtility.CombineMessages(testFailed);
            testResult.StackTrace = ExceptionUtility.CombineStackTraces(testFailed);

            TestListener.TestFinished(testResult);

            WriteOutput(testFailed.Test.DisplayName, testFailed.Output);

            return true;
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = testPassed.ToTdNetTestResult(TestState.Passed, totalTests);

            TestListener.TestFinished(testResult);

            WriteOutput(testPassed.Test.DisplayName, testPassed.Output);

            return true;
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = testSkipped.ToTdNetTestResult(TestState.Ignored, totalTests);

            testResult.Message = testSkipped.Reason;

            TestListener.TestFinished(testResult);

            return true;
        }

        protected override bool Visit(IErrorMessage error)
        {
            ReportError("Fatal Error", error);

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

        void WriteOutput(string name, string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            TestListener.WriteLine(string.Format("Output from {0}:", name), Category.Output);
            foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                TestListener.WriteLine(string.Format("  {0}", line), Category.Output);
        }
    }
}