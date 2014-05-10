using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xunit.Abstractions;
using Xunit.Runners.UI;

namespace Xunit.Runner.iOS.Visitors
{
    class MonoTestExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        private readonly Dictionary<ITestCase, MonoTestCase> testCases;
        private readonly ITestListener listener;
        private readonly Func<bool> cancelledThunk;

        public MonoTestExecutionVisitor(Dictionary<ITestCase, MonoTestCase> testCases, ITestListener listener, Func<bool> cancelledThunk)
        {
            if (testCases == null) throw new ArgumentNullException("testCases");
            if (listener == null) throw new ArgumentNullException("listener");
            if (cancelledThunk == null) throw new ArgumentNullException("cancelledThunk");

            this.testCases = testCases;
            this.listener = listener;
            this.cancelledThunk = cancelledThunk;
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var result = MakeMonoTestResult(testFailed, TestState.Failed);
            result.ErrorMessage = ExceptionUtility.CombineMessages(testFailed);
            result.ErrorStackTrace = ExceptionUtility.CombineStackTraces(testFailed);

            listener.RecordResult(result);

            return !cancelledThunk();
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            var result = MakeMonoTestResult(testPassed, TestState.Passed);
            listener.RecordResult(result);

            return !cancelledThunk();
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            var result = MakeMonoTestResult(testSkipped, TestState.Skipped);
            listener.RecordResult(result);

            return !cancelledThunk();
        }

        private MonoTestResult MakeMonoTestResult(ITestResultMessage testResult, TestState outcome)
        {
            var testCase = testCases[testResult.TestCase];
            var fqTestMethodName = String.Format("{0}.{1}", testResult.TestCase.Class.Name, testResult.TestCase.Method.Name);
            var displayName = TouchOptions.Current.GetDisplayName(testResult.TestDisplayName, testResult.TestCase.Method.Name, fqTestMethodName);

            var result = new MonoTestResult(testCase, testResult)
            {
                DisplayName = displayName,
                Duration = TimeSpan.FromSeconds((double)testResult.ExecutionTime),
                Outcome = outcome,
            };

            // Work around VS considering a test "not run" when the duration is 0
            if (result.Duration.TotalMilliseconds == 0)
                result.Duration = TimeSpan.FromMilliseconds(1);

            if (!String.IsNullOrEmpty(testResult.Output))
                result.StandardOutput = testResult.Output;

            return result;
        }
    }
}