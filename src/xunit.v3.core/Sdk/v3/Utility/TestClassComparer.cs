using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="_ITestClass"/>.
	/// Compares the fully qualified names of the types.
	/// </summary>
	public class TestClassComparer : IEqualityComparer<_ITestClass>
	{
		/// <summary>
		/// The singleton instance of the comparer.
		/// </summary>
		public static readonly TestClassComparer Instance = new TestClassComparer();

		/// <inheritdoc/>
		public bool Equals(_ITestClass? x, _ITestClass? y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.Class.Name == y.Class.Name;
		}

		/// <inheritdoc/>
		public int GetHashCode(_ITestClass obj) =>
			obj.Class.Name.GetHashCode();
	}
}
