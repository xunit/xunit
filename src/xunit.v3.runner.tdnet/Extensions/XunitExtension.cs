using System;
using System.Reflection;
using TestDriven.Framework;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
	public static class XunitExtension
	{
		public static Type? GetClass(this ITestCase testCase) =>
			testCase.TestMethod.TestClass.Class is IReflectionTypeInfo typeInfo ? typeInfo.Type : null;

		public static MethodInfo? GetMethod(this ITestCase testCase) =>
			testCase.TestMethod.Method is IReflectionMethodInfo methodInfo ? methodInfo.MethodInfo : null;

		// TODO: Delete this when there are no more callers
		public static TestResult ToTdNetTestResult(this ITestResultMessage testResult, TestState testState, int totalTests) =>
			new TestResult
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
