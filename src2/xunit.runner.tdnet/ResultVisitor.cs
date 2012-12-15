using System.Reflection;
using System.Threading;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public class ResultVisitor : TestMessageVisitor, IMessageSink
    {
        public ResultVisitor(ITestListener listener, Assembly assembly)
        {
            TestListener = listener;
            Assembly = assembly;
            Finished = new ManualResetEvent(initialState: false);
            TestRunState = TestRunState.NoTests;
        }

        public Assembly Assembly { get; private set; }
        public ManualResetEvent Finished { get; private set; }
        public ITestListener TestListener { get; private set; }
        public TestRunState TestRunState { get; set; }

        public void Dispose() { }

        public void OnMessage(ITestMessage message)
        {
            Visit(message);
        }

        protected override void Visit(ITestAssemblyFinished finished)
        {
            Finished.Set();
        }

        protected override void Visit(ITestFailed failed)
        {
            TestRunState = TestRunState.Failure;

            TestResult testResult = failed.ToTdNetTestResult(TestState.Failed);

            testResult.Message = failed.Exception.Message;
            testResult.StackTrace = failed.Exception.StackTrace;

            TestListener.TestFinished(testResult);

            //WriteOutput(name, output);
        }

        protected override void Visit(ITestPassed passed)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            TestResult testResult = passed.ToTdNetTestResult(TestState.Passed);

            TestListener.TestFinished(testResult);

            //WriteOutput(name, output);
        }

        protected override void Visit(ITestSkipped skipped)
        {
            if (TestRunState == TestRunState.NoTests)
                TestRunState = TestRunState.Success;

            TestResult testResult = skipped.ToTdNetTestResult(TestState.Ignored);

            testResult.Message = skipped.Reason;

            TestListener.TestFinished(testResult);
        }
    }
}