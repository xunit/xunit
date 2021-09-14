using System;
using System.Reflection;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
    public static class XunitExtension
    {
        public static Type GetClass(this ITestCase testCase)
        {
            var typeInfo = testCase.TestMethod.TestClass.Class as IReflectionTypeInfo;
            return typeInfo == null ? null : typeInfo.Type;
        }

        public static MethodInfo GetMethod(this ITestCase testCase)
        {
            var methodInfo = testCase.TestMethod.Method as IReflectionMethodInfo;
            return methodInfo == null ? null : methodInfo.MethodInfo;
        }

        public static TestResult ToTdNetTestResult(this ITestResultMessage testResult, TestState testState, int totalTests)
        {
            return new TestResult
            {
                FixtureType = testResult.TestCase.GetClass(),
                Method = testResult.TestCase.GetMethod(),
                Name = testResult.Test.DisplayName,
                State = testState,
                TimeSpan = new TimeSpan((long)(10000.0M * testResult.ExecutionTime)),
                TotalTests = totalTests,
            };
        }
    }
}
