using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="_ITestCollection"/>.
/// Compares the IDs of the test collections.
/// </summary>
/// <typeparam name="TTestCollection">The type of the test collection. Must derive
/// from <see cref="_ITestCollection"/>.</typeparam>
public class TestCollectionComparer<TTestCollection> : IEqualityComparer<TTestCollection>
	where TTestCollection : class, _ITestCollection
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestCollectionComparer<TTestCollection> Instance = new();

	/// <inheritdoc/>
	public bool Equals(
		TTestCollection? x,
		TTestCollection? y)
	{
		if (x is null && y is null)
			return true;
		if (x is null || y is null)
			return false;

		return x.UniqueID == y.UniqueID;
	}

	/// <inheritdoc/>
	public int GetHashCode(TTestCollection obj)
	{
		Guard.ArgumentNotNull(obj);

		return obj.UniqueID.GetHashCode();
	}
}
