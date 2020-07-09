using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="ITestClass"/>.
    /// Compares the fully qualified names of the types.
    /// </summary>
    public class TestClassComparer : IEqualityComparer<ITestClass>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        public static readonly TestClassComparer Instance = new TestClassComparer();

        /// <inheritdoc/>
        public bool Equals(ITestClass? x, ITestClass? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Class.Name == y.Class.Name;
        }

        /// <inheritdoc/>
        public int GetHashCode(ITestClass obj) =>
            obj.Class.Name.GetHashCode();
    }
}
