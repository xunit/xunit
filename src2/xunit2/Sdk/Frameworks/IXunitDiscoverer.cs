using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface IXunitDiscoverer
    {
        IEnumerable<IXunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute);
    }
}
