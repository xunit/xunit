using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit.Abstractions;
using Xunit.Runner.VisualStudio.Settings;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Xunit.Runner.VisualStudio.TestAdapter
{
    public class VsExecutionVisitor : TestMessageVisitor<ITestAssemblyFinished>
    {
        readonly Func<bool> cancelledThunk;
        readonly ITestExecutionRecorder recorder;
        readonly Dictionary<ITestCase, TestCase> testCases;
        readonly XunitVisualStudioSettings settings;

        public VsExecutionVisitor(ITestExecutionRecorder recorder, Dictionary<ITestCase, TestCase> testCases, Func<bool> cancelledThunk)
        {
            this.recorder = recorder;
            this.testCases = testCases;
            this.cancelledThunk = cancelledThunk;

            settings = SettingsProvider.Load();
        }

        protected override bool Visit(IErrorMessage error)
        {
            recorder.SendMessage(TestMessageLevel.Error, String.Format("Catastrophic failure: {0}", error.Message));

            return !cancelledThunk();
        }

        protected override bool Visit(ITestFailed testFailed)
        {
            var result = MakeVsTestResult(testFailed, TestOutcome.Failed);
            result.ErrorMessage = testFailed.Message;
            result.ErrorStackTrace = testFailed.StackTrace;

            recorder.RecordEnd(result.TestCase, result.Outcome);
            recorder.RecordResult(result);

            return !cancelledThunk();
        }

        protected override bool Visit(ITestPassed testPassed)
        {
            var result = MakeVsTestResult(testPassed, TestOutcome.Passed);
            recorder.RecordEnd(result.TestCase, result.Outcome);
            recorder.RecordResult(result);

            return !cancelledThunk();
        }

        protected override bool Visit(ITestSkipped testSkipped)
        {
            var result = MakeVsTestResult(testSkipped, TestOutcome.Skipped);
            recorder.RecordEnd(result.TestCase, result.Outcome);
            recorder.RecordResult(result);

            return !cancelledThunk();
        }

        protected override bool Visit(ITestStarting testStarting)
        {
            recorder.RecordStart(testCases[testStarting.TestCase]);

            return !cancelledThunk();
        }

        private VsTestResult MakeVsTestResult(ITestResultMessage testResult, TestOutcome outcome)
        {
            var testCase = testCases[testResult.TestCase];
            var fqTestMethodName = String.Format("{0}.{1}", testResult.TestCase.Class.Name, testResult.TestCase.Method.Name);
            var displayName = settings.GetDisplayName(testResult.TestDisplayName, testResult.TestCase.Method.Name, fqTestMethodName);

            var result = new VsTestResult(testCase)
            {
                ComputerName = Environment.MachineName,
                DisplayName = displayName,
                Duration = TimeSpan.FromSeconds((double)testResult.ExecutionTime),
                Outcome = outcome,
            };

            // Work around VS considering a test "not run" when the duration is 0
            if (result.Duration.TotalMilliseconds == 0)
                result.Duration = TimeSpan.FromMilliseconds(1);

            if (!String.IsNullOrEmpty(testResult.Output))
                result.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, testResult.Output));

            return result;
        }
    }
}