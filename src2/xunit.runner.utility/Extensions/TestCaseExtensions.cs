using System;
using System.Reflection;

namespace Xunit.Abstractions
{
    public static class TestCaseExtensions
    {
        public static MethodInfo ToMethodInfo(this ITestCase testCase)
        {
            IMethodTestCase methodTestCase = testCase as IMethodTestCase;
            if (methodTestCase != null)
            {
                IReflectionMethodInfo reflectionMethodInfo = methodTestCase.Method as IReflectionMethodInfo;
                if (reflectionMethodInfo != null)
                    return reflectionMethodInfo.MethodInfo;
            }

            return null;
        }

        public static Type ToType(this ITestCase testCase)
        {
            IClassTestCase classTestCase = testCase as IClassTestCase;
            if (classTestCase != null)
            {
                IReflectionTypeInfo reflectionTypeInfo = classTestCase.Class as IReflectionTypeInfo;
                if (reflectionTypeInfo != null)
                    return reflectionTypeInfo.Type;
            }

            return null;
        }
    }
}
