using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xunit.Abstractions;

namespace Xunit.Runner.iOS
{
    class MonoTestResult
    {
        public MonoTestCase TestCase { get; private set; }
        public ITestResultMessage TestResultMessage { get; private set; }
        public string DisplayName { get; set; }
        public TimeSpan Duration { get; set; }
        public TestState Outcome { get; set; }
        public string StandardOutput { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }

        public MonoTestResult(MonoTestCase testCase, ITestResultMessage testResult)
        {
            if (testCase == null) throw new ArgumentNullException("testCase");
            TestCase = testCase;
            TestResultMessage = testResult;

            Outcome = TestState.NotRun;
            DisplayName = testCase.DisplayName;
        }
    }
}