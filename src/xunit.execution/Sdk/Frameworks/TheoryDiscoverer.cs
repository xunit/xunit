using System;
using System.Collections.Generic;
using System.IO;
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
        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            // Special case Skip, because we want a single Skip (not one per data item), and a skipped test may
            // not actually have any data (which is quasi-legal, since it's skipped).
            if (factAttribute.GetNamedArgument<string>("Skip") != null)
                return new[] { new XunitTestCase(testCollection, assembly, testClass, testMethod, factAttribute) };

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var results = new List<XunitTestCase>();

                    var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                    foreach (var dataAttribute in dataAttributes)
                    {
                        var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                        var discoverer = ExtensibilityPointFactory.GetDataDiscoverer(discovererAttribute);

                        // GetData may return null, but that's okay; we'll let the NullRef happen and then catch it
                        // down below so that we get the composite test case.
                        foreach (var dataRow in discoverer.GetData(dataAttribute, testMethod))
                        {
                            // Attempt to serialize the test case, since we need a way to uniquely identify a test
                            // and serialization is the best way to do that. If it's not serializable, this will
                            // throw and we will fall back to a single theory test case that gets its data
                            // at runtime.
                            var testCase = new XunitTestCase(testCollection, assembly, testClass, testMethod, factAttribute, dataRow);
                            SerializationHelper.Serialize(testCase);
                            results.Add(testCase);
                        }
                    }

                    // REVIEW: Could we re-write LambdaTestCase to just be for exceptions?
                    if (results.Count == 0)
                        results.Add(new LambdaTestCase(testCollection, assembly, testClass, testMethod, factAttribute,
                                                       () => { throw new InvalidOperationException(String.Format("No data found for {0}.{1}", testClass.Name, testMethod.Name)); }));

                    return results;
                }
            }
            catch
            {
                return new XunitTestCase[] { new XunitTheoryTestCase(testCollection, assembly, testClass, testMethod, factAttribute) };
            }
        }
    }
}