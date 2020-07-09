using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="ITestMethod"/>.
    /// Compares the names of the methods.
    /// </summary>
    public class TestMethodComparer : IEqualityComparer<ITestMethod>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        public static readonly TestMethodComparer Instance = new TestMethodComparer();

        /// <inheritdoc/>
        public bool Equals(ITestMethod? x, ITestMethod? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Method.Name == y.Method.Name;
        }

        /// <inheritdoc/>
        public int GetHashCode(ITestMethod obj) =>
            obj.Method.Name.GetHashCode();
    }
}
