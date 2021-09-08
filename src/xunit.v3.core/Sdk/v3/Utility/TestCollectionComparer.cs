using System.Collections.Generic;

namespace Xunit.v3
{
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
		public bool Equals(_ITestCollection? x, _ITestCollection? y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.UniqueID == y.UniqueID;
		}

		/// <inheritdoc/>
		public int GetHashCode(_ITestCollection obj) =>
			obj.UniqueID.GetHashCode();
	}
}
