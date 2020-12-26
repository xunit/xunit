using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="_ITestMethod"/>.
	/// Compares the names of the methods.
	/// </summary>
	public class TestMethodComparer : IEqualityComparer<_ITestMethod>
	{
		/// <summary>
		/// The singleton instance of the comparer.
		/// </summary>
		public static readonly TestMethodComparer Instance = new TestMethodComparer();

		/// <inheritdoc/>
		public bool Equals(_ITestMethod? x, _ITestMethod? y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.UniqueID == y.UniqueID;
		}

		/// <inheritdoc/>
		public int GetHashCode(_ITestMethod obj) =>
			obj.Method.Name.GetHashCode();
	}
}
