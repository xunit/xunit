using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="FactAttribute"/>.
    /// </summary>
    public class FactDiscoverer : IXunitTestCaseDiscoverer
    {
        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            yield return new XunitTestCase(testCollection, assembly, testClass, testMethod, factAttribute);
        }
    }
}