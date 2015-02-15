using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="TheoryAttribute"/>.
    /// </summary>
    public class TheoryDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="TheoryDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public TheoryDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault();

            // Special case Skip, because we want a single Skip (not one per data item), and a skipped test may
            // not actually have any data (which is quasi-legal, since it's skipped).
            if (factAttribute.GetNamedArgument<string>("Skip") != null)
                return new[] { new XunitTestCase(diagnosticMessageSink, defaultMethodDisplay, testMethod) };

            var dataAttributes = testMethod.Method.GetCustomAttributes(typeof(DataAttribute));

            if (discoveryOptions.PreEnumerateTheoriesOrDefault())
            {
                try
                {
                    var results = new List<XunitTestCase>();

                    foreach (var dataAttribute in dataAttributes)
                    {
                        var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                        var discoverer = ExtensibilityPointFactory.GetDataDiscoverer(diagnosticMessageSink, discovererAttribute);
                        if (!discoverer.SupportsDiscoveryEnumeration(dataAttribute, testMethod.Method))
                            return new XunitTestCase[] { new XunitTheoryTestCase(diagnosticMessageSink, defaultMethodDisplay, testMethod) };

                        // GetData may return null, but that's okay; we'll let the NullRef happen and then catch it
                        // down below so that we get the composite test case.
                        foreach (var dataRow in discoverer.GetData(dataAttribute, testMethod.Method))
                        {
                            // Attempt to serialize the test case, since we need a way to uniquely identify a test
                            // and serialization is the best way to do that. If it's not serializable, this will
                            // throw and we will fall back to a single theory test case that gets its data
                            // at runtime.
                            var testCase = new XunitTestCase(diagnosticMessageSink, defaultMethodDisplay, testMethod, dataRow);
                            SerializationHelper.Serialize(testCase);
                            results.Add(testCase);
                        }
                    }

                    if (results.Count == 0)
                        results.Add(new ExecutionErrorTestCase(diagnosticMessageSink, defaultMethodDisplay, testMethod,
                                                               String.Format("No data found for {0}.{1}", testMethod.TestClass.Class.Name, testMethod.Method.Name)));

                    return results;
                }
                catch { }  // If there are serialization issues with the theory data, fall through to return just the XunitTestCase
            }

            return new XunitTestCase[] { new XunitTheoryTestCase(diagnosticMessageSink, defaultMethodDisplay, testMethod) };
        }
    }
}