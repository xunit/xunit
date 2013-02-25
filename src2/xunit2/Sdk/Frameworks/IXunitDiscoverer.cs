using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface IXunitDiscoverer
    {
        IEnumerable<XunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute);
    }
}
