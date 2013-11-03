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

            TestResult testResult = failed.ToTdNetTestResult(TestState.Failed);

            testResult.Message = failed.Message;
            testResult.StackTrace = failed.StackTrace;

            TestListener.TestFinished(testResult);

            //WriteOutput(name, output);

            return true;
        }

        protected override bool Visit(ITestPassed passed)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            TestResult testResult = passed.ToTdNetTestResult(TestState.Passed);

            TestListener.TestFinished(testResult);

            //WriteOutput(name, output);

            return true;
        }

        protected override bool Visit(ITestSkipped skipped)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            TestResult testResult = skipped.ToTdNetTestResult(TestState.Ignored);

            testResult.Message = skipped.Reason;

            TestListener.TestFinished(testResult);

            return true;
        }
    }
}