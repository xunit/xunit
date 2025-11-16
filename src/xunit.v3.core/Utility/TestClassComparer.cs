using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="TestClassComparer{TTestClass}"/>.
/// This non-generic version will be removed in the next major version.
/// </summary>
[Obsolete("Please use the generic version of this class. This overload will be removed in the next major version.")]
[EditorBrowsable(EditorBrowsableState.Never)]
public class TestClassComparer : TestClassComparer<ITestClass> { }

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
/// for <typeparamref name="TTestClass"/>, using the unique ID for the comparison.
/// </summary>
public class TestClassComparer<TTestClass> : IEqualityComparer<TTestClass?>, IComparer<TTestClass?>
	where TTestClass : ITestClass
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestClassComparer<TTestClass> Instance = new();

	/// <inheritdoc/>
	public int Compare(
		TTestClass? x,
		TTestClass? y) =>
			string.CompareOrdinal(x?.UniqueID, y?.UniqueID);

	/// <inheritdoc/>
	public bool Equals(
		TTestClass? x,
		TTestClass? y) =>
			string.Equals(x?.UniqueID, y?.UniqueID, StringComparison.Ordinal);

	/// <inheritdoc/>
	public int GetHashCode(TTestClass? obj) =>
		obj is null ? 0 : obj.TestClassName.GetHashCode();
}
