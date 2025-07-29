using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
/// for <see cref="ITestCollection"/>, using the unique ID for the comparison.
/// </summary>
/// <typeparam name="TTestCollection">The type of the test collection. Must derive
/// from <see cref="ITestCollection"/>.</typeparam>
public class TestCollectionComparer<TTestCollection> : IEqualityComparer<TTestCollection>, IComparer<TTestCollection>
	where TTestCollection : class, ITestCollection
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestCollectionComparer<TTestCollection> Instance = new();

	/// <inheritdoc/>
	public int Compare(
		TTestCollection? x,
		TTestCollection? y) =>
			string.CompareOrdinal(x?.UniqueID, y?.UniqueID);

	/// <inheritdoc/>
	public bool Equals(
		TTestCollection? x,
		TTestCollection? y) =>
			string.Equals(x?.UniqueID, y?.UniqueID, StringComparison.Ordinal);

	/// <inheritdoc/>
	public int GetHashCode(TTestCollection obj) =>
		Guard.ArgumentNotNull(obj).UniqueID.GetHashCode();
}
