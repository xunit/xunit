using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="TestMethodComparer{TTestMethod}"/>.
/// This non-generic version will be removed in the next major version.
/// </summary>
[Obsolete("Please use the generic version of this class. This overload will be removed in the next major version.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TestMethodComparer : TestMethodComparer<ITestMethod> { }

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
/// for <typeparamref name="TTestMethod"/>, using the unique ID for the comparison.
/// </summary>
public class TestMethodComparer<TTestMethod> : IEqualityComparer<TTestMethod?>, IComparer<TTestMethod?>
	where TTestMethod : ITestMethod
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestMethodComparer<TTestMethod> Instance = new();

	/// <inheritdoc/>
	public int Compare(
		TTestMethod? x,
		TTestMethod? y) =>
			string.CompareOrdinal(x?.UniqueID, y?.UniqueID);

	/// <inheritdoc/>
	public bool Equals(
		TTestMethod? x,
		TTestMethod? y) =>
			string.Equals(x?.UniqueID, y?.UniqueID, StringComparison.Ordinal);

	/// <inheritdoc/>
	public int GetHashCode(TTestMethod? obj) =>
		obj is null ? 0 : obj.MethodName.GetHashCode();
}
