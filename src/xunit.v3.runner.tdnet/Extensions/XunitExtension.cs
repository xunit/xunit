using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.v3;

namespace Xunit.Runner.TdNet
{
	public static class XunitExtension
	{
		public static Type? GetClass(this _ITestCase testCase) =>
			testCase.TestMethod.TestClass.Class is IReflectionTypeInfo typeInfo ? typeInfo.Type : null;

		public static MethodInfo? GetMethod(this _ITestCase testCase) =>
			testCase.TestMethod.Method is IReflectionMethodInfo methodInfo ? methodInfo.MethodInfo : null;
	}
}
