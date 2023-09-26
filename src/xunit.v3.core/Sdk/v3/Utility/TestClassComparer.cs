using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="_ITestClass"/>.
/// Compares the fully qualified names of the types.
/// </summary>
public class TestClassComparer : IEqualityComparer<_ITestClass?>
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestClassComparer Instance = new();

	/// <inheritdoc/>
	public bool Equals(
		_ITestClass? x,
		_ITestClass? y)
	{
		if (x is null && y is null)
			return true;
		if (x is null || y is null)
			return false;

		return x.UniqueID == y.UniqueID;
	}

	/// <inheritdoc/>
	public int GetHashCode(_ITestClass? obj) =>
		obj is null ? 0 : obj.Class.Name.GetHashCode();
}
