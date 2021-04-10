using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// Default implementation of <see cref="ITestCollectionOrderer"/>. Orders tests in
	/// an unpredictable and unstable order, so that repeated test runs of the
	/// identical test assembly run test collections in a random order.
	/// </summary>
	public class DefaultTestCollectionOrderer : ITestCollectionOrderer
	{
		/// <inheritdoc/>
		public IReadOnlyCollection<_ITestCollection> OrderTestCollections(IReadOnlyCollection<_ITestCollection> testCollections)
		{
			Guard.ArgumentNotNull(nameof(testCollections), testCollections);

			var result = testCollections.ToList();
			result.Sort(Compare);
			return result;
		}

		int Compare<TTestCollection>(TTestCollection x, TTestCollection y)
			where TTestCollection : _ITestCollection
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
