using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IDataDiscoverer"/> used to discover the data
    /// provided by <see cref="InlineDataAttribute"/>.
    /// </summary>
    public class InlineDataDiscoverer : IDataDiscoverer
    {
        /// <inheritdoc/>
        public virtual IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            // The data from GetConstructorArguments does not maintain its original form (in particular, collections
            // end up as generic IEnumerable<T>). So we end up needing to call .ToArray() on the enumerable so that
            // we can restore the correct argument type from InlineDataAttribute.
            //
            // In addition, [InlineData(null)] gets translated into passing a null array, not a single array with a null
            // value in it, which is why the null coalesce operator is required (this is covered by the acceptance test
            // in Xunit2TheoryAcceptanceTests.InlineDataTests.SingleNullValuesWork).

            var args = (IEnumerable<object>)dataAttribute.GetConstructorArguments().Single() ?? new object[] { null };
            return new[] { args.ToArray() };
        }

        /// <inheritdoc/>
        public virtual bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            return true;
        }
    }
}
