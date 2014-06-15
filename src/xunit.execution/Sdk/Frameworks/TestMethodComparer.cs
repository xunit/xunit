using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="IMethodInfo"/>.
    /// Compares the names of the methods.
    /// </summary>
    public class TestMethodComparer : IEqualityComparer<IMethodInfo>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        public static readonly TestMethodComparer Instance = new TestMethodComparer();

        /// <inheritdoc/>
        public bool Equals(IMethodInfo x, IMethodInfo y)
        {
            return x.Name == y.Name;
        }

        /// <inheritdoc/>
        public int GetHashCode(IMethodInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
