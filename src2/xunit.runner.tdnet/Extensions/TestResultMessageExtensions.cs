using System;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public static class TestResultMessageExtensions
    {
        public static TestResult ToTdNetTestResult(this ITestResultMessage testResult, TestState testState)
        {
            return new TestResult
            {
                FixtureType = testResult.TestCase.Class,
                Method = testResult.TestCase.Method,
                Name = testResult.TestDisplayName,
                State = testState,
                TimeSpan = new TimeSpan((long)(10000.0M * testResult.ExecutionTime)),
                TotalTests = 1,
            };
        }
    }
}