using System;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class ResultVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        public ResultVisitor(ITestListener listener)
        {
            TestListener = listener;
            TestRunState = TestRunState.NoTests;
        }

        public ITestListener TestListener { get; private set; }
        public TestRunState TestRunState { get; set; }

        protected override bool Visit(ITestFailed failed)
        {
            TestRunState = TestRunState.Failure;

            var testResult = failed.ToTdNetTestResult(TestState.Failed);

            testResult.Message = ExceptionUtility.CombineMessages(failed);
            testResult.StackTrace = ExceptionUtility.CombineStackTraces(failed);

            TestListener.TestFinished(testResult);

            WriteOutput(failed.TestDisplayName, failed.Output);

            return true;
        }

        protected override bool Visit(ITestPassed passed)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = passed.ToTdNetTestResult(TestState.Passed);

            TestListener.TestFinished(testResult);

            WriteOutput(passed.TestDisplayName, passed.Output);

            return true;
        }

        protected override bool Visit(ITestSkipped skipped)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            var testResult = skipped.ToTdNetTestResult(TestState.Ignored);

            testResult.Message = skipped.Reason;

            TestListener.TestFinished(testResult);

            return true;
        }

        private void WriteOutput(string name, string output)
        {
            if (String.IsNullOrWhiteSpace(output))
                return;

            TestListener.WriteLine(String.Format("Output from {0}:", name), Category.Output);
            foreach (string line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                TestListener.WriteLine(String.Format("  {0}", line), Category.Output);
        }
    }
}