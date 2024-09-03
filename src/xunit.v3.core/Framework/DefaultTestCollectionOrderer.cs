using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ITestCollectionOrderer"/>. Orders tests in
/// an unpredictable and unstable order, so that repeated test runs of the
/// identical test assembly run test collections in a random order.
/// </summary>
public class DefaultTestCollectionOrderer : ITestCollectionOrderer
{
	DefaultTestCollectionOrderer()
	{ }

	/// <summary>
	/// Get the singleton instance of <see cref="DefaultTestCollectionOrderer"/>.
	/// </summary>
	public static DefaultTestCollectionOrderer Instance { get; } = new();

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
		where TTestCollection : ITestCollection
	{
		Guard.ArgumentNotNull(testCollections);

		var result = testCollections.ToList();
		result.Sort(Compare);
		return result;
	}

	static int Compare<TTestCollection>(
		TTestCollection x,
		TTestCollection y)
			where TTestCollection : ITestCollection
	{
		var xHash = x.UniqueID.GetHashCode();
		var yHash = y.UniqueID.GetHashCode();

		return
			xHash == yHash
			? 0
			: xHash < yHash
				? -1
				: 1;
	}
}
