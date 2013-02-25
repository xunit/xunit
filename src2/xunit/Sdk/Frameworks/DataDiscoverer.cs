using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class DataDiscoverer : IDataDiscoverer
    {
        public IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            var reflectionDataAttribute = dataAttribute as IReflectionAttributeInfo;
            var reflectionTestMethod = testMethod as IReflectionMethodInfo;

            if (reflectionDataAttribute != null && reflectionTestMethod != null)
                return ((DataAttribute)reflectionDataAttribute.Attribute).GetData(reflectionTestMethod.MethodInfo);

            return null;
        }
    }
}