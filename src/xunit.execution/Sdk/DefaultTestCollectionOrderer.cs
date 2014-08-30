using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestCollectionOrderer"/>. Orders tests in
    /// an unpredictable and unstable order, so that repeated test runs of the
    /// identical test assembly run test collections in a random order.
    /// </summary>
    public class DefaultTestCollectionOrderer : ITestCollectionOrderer
    {
        /// <inheritdoc/>
        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> TestCollections)
        {
            var result = TestCollections.ToList();
            result.Sort(Compare);
            return result;
        }

        int Compare<TTestCollection>(TTestCollection x, TTestCollection y)
            where TTestCollection : ITestCollection
        {
            var xHash = x.UniqueID.GetHashCode();
            var yHash = y.UniqueID.GetHashCode();

            if (xHash == yHash)
                return 0;
            if (xHash < yHash)
                return -1;
            return 1;
        }
    }
}
