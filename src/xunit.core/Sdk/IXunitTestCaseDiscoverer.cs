using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface to be implemented by classes which are used to discover tests cases attached
    /// to test methods that are attributed with <see cref="FactAttribute"/> (or a subclass).
    /// </summary>
    public interface IXunitTestCaseDiscoverer
    {
        /// <summary>
        /// Discover test cases from a test method.
        /// </summary>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="testMethod">The test method the test cases belong to.</param>
        /// <param name="factAttribute">The fact attribute attached to the test method.</param>
        /// <returns>Returns zero or more test cases represented by the test method.</returns>
        IEnumerable<IXunitTestCase> Discover(TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, IAttributeInfo factAttribute);
    }
}