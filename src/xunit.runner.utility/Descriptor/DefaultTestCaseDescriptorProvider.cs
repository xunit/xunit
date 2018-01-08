using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// INTERNAL CLASS. DO NOT USE.
    /// </summary>
    public class DefaultTestCaseDescriptorProvider : ITestCaseDescriptorProvider
    {
        readonly ITestFrameworkDiscoverer discoverer;

        /// <summary/>
        public DefaultTestCaseDescriptorProvider(ITestFrameworkDiscoverer discoverer)
        {
            this.discoverer = discoverer;
        }

        /// <inheritdoc/>
        public List<TestCaseDescriptor> GetTestCaseDescriptors(List<ITestCase> testCases, bool includeSerialization)
        {
            var results = new List<TestCaseDescriptor>(testCases.Count);

            foreach (var testCase in testCases)
            {
                var serialization = includeSerialization && discoverer != null ? discoverer.Serialize(testCase) : null;
                var sourceInformation = testCase.SourceInformation;
                var testMethod = testCase.TestMethod;

                try
                {
                    results.Add(new TestCaseDescriptor
                    {
                        ClassName = testMethod.TestClass.Class.Name,
                        DisplayName = testCase.DisplayName,
                        MethodName = testMethod.Method.Name,
                        Serialization = serialization,
                        SkipReason = testCase.SkipReason,
                        SourceFileName = sourceInformation?.FileName,
                        SourceLineNumber = sourceInformation?.LineNumber,
                        Traits = testCase.Traits ?? new Dictionary<string, List<string>>(),
                        UniqueID = testCase.UniqueID
                    });
                }
                catch (Exception) { }
            }

            return results;
        }
    }
}
