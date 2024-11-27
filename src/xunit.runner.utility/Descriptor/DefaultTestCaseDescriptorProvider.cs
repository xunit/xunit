using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// INTERNAL CLASS. DO NOT USE.
    /// </summary>
    public class DefaultTestCaseDescriptorProvider : ITestCaseDescriptorProvider
    {
        readonly ITestFrameworkDiscoverer discoverer;
        readonly IMessageSink diagnosticMessageSink;

        /// <summary/>
        public DefaultTestCaseDescriptorProvider(ITestFrameworkDiscoverer discoverer, IMessageSink diagnosticMessageSink)
        {
            this.discoverer = discoverer;
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        public List<TestCaseDescriptor> GetTestCaseDescriptors(List<ITestCase> testCases, bool includeSerialization)
        {
            var results = new List<TestCaseDescriptor>(testCases.Count);

            foreach (var testCase in testCases)
            {
                var sourceInformation = testCase.SourceInformation;
                var testMethod = testCase.TestMethod;
                var className = testMethod.TestClass.Class.Name;
                var methodName = testMethod.Method.Name;

                var displayName = $"{className}.{methodName}";
                Dictionary<string, List<string>> traits = null;

                try
                {
                    var serialization = includeSerialization && discoverer != null ? discoverer.Serialize(testCase) : null;
                    displayName = testCase.DisplayName;
                    traits = testCase.Traits;

                    results.Add(new TestCaseDescriptor
                    {
                        ClassName = className,
                        DisplayName = displayName,
                        MethodName = methodName,
                        Serialization = serialization,
                        SkipReason = testCase.SkipReason,
                        SourceFileName = sourceInformation?.FileName,
                        SourceLineNumber = sourceInformation?.LineNumber,
                        Traits = traits ?? new Dictionary<string, List<string>>(),
                        UniqueID = testCase.UniqueID
                    });
                }
                catch (Exception e)
                {
                    diagnosticMessageSink.OnMessage(
                        new DiagnosticMessage(
                            "Creating the descriptor for '{0}' threw '{1}': {2}{3}{4}",
                            displayName,
                            e.GetType().FullName,
                            e.Message,
                            Environment.NewLine,
                            e.StackTrace
                        )
                    );

                    results.Add(new TestCaseDescriptor
                    {
                        ClassName = className,
                        DisplayName = displayName,
                        MethodName = methodName,
                        SkipReason = e.Message,
                        Traits = traits ?? new Dictionary<string, List<string>>(),
                    });
                }
            }

            return results;
        }
    }
}
