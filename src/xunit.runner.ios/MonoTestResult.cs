using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit.Abstractions;

namespace Xunit.Runners
{
    public class MonoTestResult
    {
        public MonoTestCase TestCase { get; private set; }
        public ITestResultMessage TestResultMessage { get; private set; }
        public TimeSpan Duration { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorStackTrace { get; set; }

        public MonoTestResult(MonoTestCase testCase, ITestResultMessage testResult)
        {
            if (testCase == null) throw new ArgumentNullException("testCase");
            TestCase = testCase;
            TestResultMessage = testResult;

            if(testResult != null)
                testCase.UpdateTestState(this);
        }

        internal void RaiseTestUpdated()
        {
            TestCase.RaiseTestCaseUpdated();
        }
    }
}