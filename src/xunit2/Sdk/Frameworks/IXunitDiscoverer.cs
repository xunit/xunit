using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface to be implemented by classes which are used to discover tests cases attached
    /// to test methods that are attributed with <see cref="FactAttribute"/> (or a subclass).
    /// </summary>
    public interface IXunitDiscoverer
    {
        /// <summary>
        /// Discover test cases from a test method.
        /// </summary>
        /// <param name="testCollection">The test collection the test cases belong to.</param>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="testClass">The test class.</param>
        /// <param name="testMethod">The test method.</param>
        /// <param name="factAttribute">The fact attribute attached to the test method.</param>
        /// <returns>Returns zero or more test cases represented by the test method.</returns>
        IEnumerable<XunitTestCase> Discover(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute);
    }
}