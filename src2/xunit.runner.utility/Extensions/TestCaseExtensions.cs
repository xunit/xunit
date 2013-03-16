using System;
using System.Reflection;

namespace Xunit.Abstractions
{
    public static class TestCaseExtensions
    {
        [Obsolete]
        public static MethodInfo ToMethodInfo(this ITestCase testCase)
        {
            if (testCase == null)
                return null;

            return testCase.Method;
        }

        [Obsolete]
        public static Type ToType(this ITestCase testCase)
        {
            if (testCase == null)
                return null;

            return testCase.Class;
        }
    }
}
