using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class FactDiscoverer : IXunitDiscoverer
    {
        public IEnumerable<IXunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            yield return new XunitTestCase(assembly, testClass, testMethod, factAttribute);
        }
    }
}