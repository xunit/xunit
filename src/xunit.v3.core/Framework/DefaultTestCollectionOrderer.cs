using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ITestCollectionOrderer"/>. Orders tests in
/// an unpredictable and unstable order, so that repeated test runs of the
/// identical test assembly run test collections in a random order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class DefaultTestCollectionOrderer() : ITestCollectionOrderer
{
	/// <summary>
	/// Get the singleton instance of <see cref="DefaultTestCollectionOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static DefaultTestCollectionOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

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
		Guard.ArgumentNotNull(x.UniqueID);
		Guard.ArgumentNotNull(y.UniqueID);

		return string.CompareOrdinal(x.UniqueID, y.UniqueID);
	}
}
