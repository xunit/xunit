using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit.Abstractions;

namespace Xunit.Runner.VisualStudio
{
    public static class TestCaseMapper
    {
        static Dictionary<string, Dictionary<TestCase, ITestCase>> testCaseMapping = new Dictionary<string, Dictionary<TestCase, ITestCase>>();

        public static void Clear(string source)
        {
            testCaseMapping.Remove(source);
        }

        public static ITestCase Find(string source, TestCase vsTestCase)
        {
            ITestCase result;
            Lookup(source).TryGetValue(vsTestCase, out result);
            return result;
        }

        public static void Set(string source, TestCase vsTestCase, ITestCase xunitTestCase)
        {
            Lookup(source)[vsTestCase] = xunitTestCase;
        }

        static Dictionary<TestCase, ITestCase> Lookup(string source)
        {
            Dictionary<TestCase, ITestCase> result;

            if (!testCaseMapping.TryGetValue(source, out result))
            {
                result = new Dictionary<TestCase, ITestCase>();
                testCaseMapping[source] = result;
            }

            return result;
        }
    }
}
