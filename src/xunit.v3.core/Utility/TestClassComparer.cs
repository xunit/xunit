using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="IEqualityComparer{T}"/> and <see cref="IComparer{T}"/>
/// for <see cref="ITestClass"/>, using the unique ID for the comparison.
/// </summary>
public class TestClassComparer : IEqualityComparer<ITestClass?>, IComparer<ITestClass?>
{
	/// <summary>
	/// The singleton instance of the comparer.
	/// </summary>
	public static readonly TestClassComparer Instance = new();

	/// <inheritdoc/>
	public int Compare(
		ITestClass? x,
		ITestClass? y) =>
			string.CompareOrdinal(x?.UniqueID, y?.UniqueID);

	/// <inheritdoc/>
	public bool Equals(
		ITestClass? x,
		ITestClass? y) =>
			string.Equals(x?.UniqueID, y?.UniqueID, StringComparison.Ordinal);

	/// <inheritdoc/>
	public int GetHashCode(ITestClass? obj) =>
		obj is null ? 0 : obj.TestClassName.GetHashCode();
}
