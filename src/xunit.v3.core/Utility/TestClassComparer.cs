using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="ITestClass"/>.
/// Compares the fully qualified names of the types.
/// </summary>
public class TestClassComparer : IEqualityComparer<ITestClass?>
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestClassComparer Instance = new();

	/// <inheritdoc/>
	public bool Equals(
		ITestClass? x,
		ITestClass? y)
	{
		if (x is null && y is null)
			return true;
		if (x is null || y is null)
			return false;

		return x.UniqueID == y.UniqueID;
	}

	/// <inheritdoc/>
	public int GetHashCode(ITestClass? obj) =>
		obj is null ? 0 : obj.TestClassName.GetHashCode();
}
