using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TheoryDiscoverer : IXunitDiscoverer
    {
        public IEnumerable<XunitTestCase> Discover(IAssemblyInfo assembly, ITypeInfo testClass, IMethodInfo testMethod, IAttributeInfo factAttribute)
        {
            foreach (IAttributeInfo inlineDataAttribute in testMethod.GetCustomAttributes(typeof(InlineDataAttribute)))
                yield return new XunitTestCase(assembly, testClass, testMethod, factAttribute, (IEnumerable<object>)inlineDataAttribute.GetConstructorArguments().Single());
        }
    }
}
