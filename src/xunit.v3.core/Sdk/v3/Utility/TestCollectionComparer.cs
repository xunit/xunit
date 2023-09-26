using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="_ITestCollection"/>.
/// Compares the IDs of the test collections.
/// </summary>
public class TestCollectionComparer : IEqualityComparer<_ITestCollection>
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestCollectionComparer Instance = new();

	/// <inheritdoc/>
	public bool Equals(
		_ITestCollection? x,
		_ITestCollection? y)
	{
		if (x is null && y is null)
			return true;
		if (x is null || y is null)
			return false;

		return x.UniqueID == y.UniqueID;
	}

	/// <inheritdoc/>
	public int GetHashCode(_ITestCollection obj)
	{
		Guard.ArgumentNotNull(obj);

		return obj.UniqueID.GetHashCode();
	}
}
