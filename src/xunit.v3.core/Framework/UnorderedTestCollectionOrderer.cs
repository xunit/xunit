using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="ITestCollectionOrderer"/> that does not change the order.
/// </summary>
[method: Obsolete("Please use the singleton instance available via the Instance property")]
[method: EditorBrowsable(EditorBrowsableState.Never)]
public class UnorderedTestCollectionOrderer() : ITestCollectionOrderer
{
	/// <summary>
	/// Get the singleton instance of <see cref="UnorderedTestCollectionOrderer"/>.
	/// </summary>
#pragma warning disable CS0618 // Type or member is obsolete
	public static UnorderedTestCollectionOrderer Instance { get; } = new();
#pragma warning restore CS0618 // Type or member is obsolete

	/// <inheritdoc/>
	public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
		where TTestCollection : ITestCollection =>
			testCollections;
}
