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

        /// <summary>
        /// Creates a test case for a single row of data. By default, returns an instance of <see cref="XunitTestCase"/>
        /// with the data row inside of it.
        /// </summary>
        /// <param name="discoveryOptions">The discovery options to be used.</param>
        /// <param name="testMethod">The test method the test cases belong to.</param>
        /// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
        /// <param name="dataRow">The row of data for this test case.</param>
        /// <returns>The test case</returns>
        protected virtual IXunitTestCase CreateTestCaseForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
            => new XunitTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow);

        /// <summary>
        /// Creates a test case for a skipped theory. By default, returns an instance of <see cref="XunitTestCase"/>
        /// (which inherently discovers the skip reason via the fact attribute).
        /// </summary>
        /// <param name="discoveryOptions">The discovery options to be used.</param>
        /// <param name="testMethod">The test method the test cases belong to.</param>
        /// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
        /// <param name="skipReason">The skip reason that decorates <paramref name="theoryAttribute"/>.</param>
        /// <returns>The test case</returns>
        protected virtual IXunitTestCase CreateTestCaseForSkip(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, string skipReason)
            => new XunitTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);

        /// <summary>
        /// Creates a test case for the entire theory. This is used when one or more of the theory data items
        /// are not serializable, or if the user has requested to skip theory pre-enumeration. By default,
        /// returns an instance of <see cref="XunitTheoryTestCase"/>, which performs the data discovery at runtime.
        /// </summary>
        /// <param name="discoveryOptions">The discovery options to be used.</param>
        /// <param name="testMethod">The test method the test cases belong to.</param>
        /// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
        /// <returns>The test case</returns>
        protected virtual IXunitTestCase CreateTestCaseForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
            => new XunitTheoryTestCase(diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);

        /// <summary>
        /// Discover test cases from a test method.
        /// </summary>
        /// <remarks>
        /// This method performs the following steps:
        /// - If the theory attribute is marked with Skip, returns the single test case from <see cref="CreateTestCaseForSkip"/>;
        /// - If pre-enumeration is off, or any of the test data is non serializable, returns the single test case from <see cref="CreateTestCaseForTheory"/>;
        /// - If there is no theory data, returns a single test case of <see cref="ExecutionErrorTestCase"/> with the error in it;
        /// - Otherwise, it returns one test case per data row, created by calling <see cref="CreateTestCaseForDataRow"/>.
        /// </remarks>
        /// <param name="discoveryOptions">The discovery options to be used.</param>
        /// <param name="testMethod">The test method the test cases belong to.</param>
        /// <param name="theoryAttribute">The theory attribute attached to the test method.</param>
        /// <returns>Returns zero or more test cases represented by the test method.</returns>
        public virtual IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
        {
            // Special case Skip, because we want a single Skip (not one per data item); plus, a skipped test may
            // not actually have any data (which is quasi-legal, since it's skipped).
            var skipReason = theoryAttribute.GetNamedArgument<string>("Skip");
            if (skipReason != null)
                return new[] { CreateTestCaseForSkip(discoveryOptions, testMethod, theoryAttribute, skipReason) };

            if (discoveryOptions.PreEnumerateTheoriesOrDefault())
            {
                try
                {
                    var dataAttributes = testMethod.Method.GetCustomAttributes(typeof(DataAttribute));
                    var results = new List<IXunitTestCase>();

                    foreach (var dataAttribute in dataAttributes)
                    {
                        var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                        var discoverer = ExtensibilityPointFactory.GetDataDiscoverer(diagnosticMessageSink, discovererAttribute);
                        if (!discoverer.SupportsDiscoveryEnumeration(dataAttribute, testMethod.Method))
                            return new[] { CreateTestCaseForTheory(discoveryOptions, testMethod, theoryAttribute) };

                        // GetData may return null, but that's okay; we'll let the NullRef happen and then catch it
                        // down below so that we get the composite test case.
                        foreach (var dataRow in discoverer.GetData(dataAttribute, testMethod.Method))
                        {
                            // Determine whether we can serialize the test case, since we need a way to uniquely
                            // identify a test and serialization is the best way to do that. If it's not serializable,
                            // this will throw and we will fall back to a single theory test case that gets its data at runtime.
                            if (!SerializationHelper.IsSerializable(dataRow))
                                return new[] { CreateTestCaseForTheory(discoveryOptions, testMethod, theoryAttribute) };

                            var testCase = CreateTestCaseForDataRow(discoveryOptions, testMethod, theoryAttribute, dataRow);
                            results.Add(testCase);
                        }
                    }

                    if (results.Count == 0)
                        results.Add(new ExecutionErrorTestCase(diagnosticMessageSink,
                                                               discoveryOptions.MethodDisplayOrDefault(),
                                                               testMethod,
                                                               $"No data found for {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}"));

                    return results;
                }
                catch { }  // If something goes wrong, fall through to return just the XunitTestCase
            }

            return new[] { CreateTestCaseForTheory(discoveryOptions, testMethod, theoryAttribute) };
        }
    }
}