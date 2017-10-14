using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// INTERNAL CLASS. DO NOT USE.
    /// </summary>
    public class TestCaseDescriptorFactory : LongLivedMarshalByRefObject
    {
        // Same as TestCaseDescriptor
        const string Separator = "\n";
        const string SeparatorEscape = "\\n";

        static Dictionary<string, List<string>> EmptyTraits = new Dictionary<string, List<string>>();
        static Type XunitTestCaseType = typeof(XunitTestCase);

        /// <summary/>
        public TestCaseDescriptorFactory(object discovererObject, object testCasesObject, object callbackObject)
        {
            var discoverer = (ITestFrameworkDiscoverer)discovererObject;
            var testCases = (List<ITestCase>)testCasesObject;
            var callback = (Action<List<string>>)callbackObject;
            var results = new List<string>(testCases.Count);

            foreach (var testCase in testCases)
            {
                var result = new StringBuilder();
                var className = testCase.TestMethod?.TestClass?.Class?.Name;
                var methodName = testCase.TestMethod?.Method?.Name;

                result.AppendFormat("C {1}{0}M {2}{0}U {3}{0}D {4}{0}",
                                    Separator,
                                    className,
                                    methodName,
                                    testCase.UniqueID,
                                    Encode(testCase.DisplayName));

                if (discoverer != null)
                {
                    var serialization = default(string);

                    if (testCase.GetType() == XunitTestCaseType)
                    {
                        var xunitTestCase = (XunitTestCase)testCase;
                        if (xunitTestCase.TestMethodArguments == null || xunitTestCase.TestMethodArguments.Length == 0)
                            serialization = $":F:{className}:{methodName}:{(int)xunitTestCase.DefaultMethodDisplay}";
                    }

                    if (serialization == null)
                        serialization = discoverer.Serialize(testCase);

                    result.AppendFormat("S {1}{0}",
                                        Separator,
                                        serialization);
                }

                if (!string.IsNullOrEmpty(testCase.SkipReason))
                    result.AppendFormat("R {1}{0}",
                                        Separator,
                                        Encode(testCase.SkipReason));

                if (!string.IsNullOrEmpty(testCase.SourceInformation?.FileName))
                    result.AppendFormat("F {1}{0}L {2}{0}",
                                        Separator,
                                        testCase.SourceInformation.FileName,
                                        testCase.SourceInformation.LineNumber);

                foreach (var name in testCase.Traits ?? EmptyTraits)
                    foreach (var value in name.Value)
                        result.AppendFormat("T {1}{0}{2}{0}",
                                            Separator,
                                            Encode(name.Key),
                                            Encode(value));

                results.Add(result.ToString());
            }

            callback(results);
        }

        static string Encode(string value)
            => value?.Replace(Separator, SeparatorEscape);
    }
}
