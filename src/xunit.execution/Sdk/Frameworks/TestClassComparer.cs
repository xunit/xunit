using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// An implementation of <see cref="IEqualityComparer{T}"/> for <see cref="ITypeInfo"/>.
    /// Compares the fully qualified names of the types.
    /// </summary>
    public class TestClassComparer : IEqualityComparer<ITypeInfo>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        public static readonly TestClassComparer Instance = new TestClassComparer();

        /// <inheritdoc/>
        public bool Equals(ITypeInfo x, ITypeInfo y)
        {
            return x.Name == y.Name;
        }

        /// <inheritdoc/>
        public int GetHashCode(ITypeInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
