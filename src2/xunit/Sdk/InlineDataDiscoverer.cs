using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class InlineDataDiscoverer : IDataDiscoverer
    {
        public IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            // The data from GetConstructorArguments does not maintain its original form (in particular, collections
            // end up as generic IEnumerable<T>). So we end up needing to call .ToArray() on the enumerable so that
            // we can restore the correct argument type from InlineDataAttribute.

            return new[] { dataAttribute.GetConstructorArguments()
                                        .Cast<IEnumerable<object>>()
                                        .Single()
                                        .ToArray() };
        }
    }
}