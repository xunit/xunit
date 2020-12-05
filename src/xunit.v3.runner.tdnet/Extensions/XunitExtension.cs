using System;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Runner.TdNet
{
	public static class XunitExtension
	{
		public static Type? GetClass(this ITestCase testCase) =>
			testCase.TestMethod.TestClass.Class is IReflectionTypeInfo typeInfo ? typeInfo.Type : null;

		public static MethodInfo? GetMethod(this ITestCase testCase) =>
			testCase.TestMethod.Method is IReflectionMethodInfo methodInfo ? methodInfo.MethodInfo : null;
	}
}
