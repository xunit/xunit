using System;
using System.Reflection;
using Xunit.v3;

namespace Xunit.Runner.TdNet
{
	public static class XunitExtension
	{
		public static Type? GetClass(this _ITestCase testCase) =>
			testCase.TestMethod.TestClass.Class is _IReflectionTypeInfo typeInfo ? typeInfo.Type : null;

		public static MethodInfo? GetMethod(this _ITestCase testCase) =>
			testCase.TestMethod.Method is _IReflectionMethodInfo methodInfo ? methodInfo.MethodInfo : null;
	}
}
