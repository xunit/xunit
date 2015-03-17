using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a reflection-backed implementation of <see cref="ITypeInfo"/>.
    /// </summary>
    public interface IReflectionTypeInfo : ITypeInfo
    {
        /// <summary>
        /// Gets the underlying <see cref="Type"/> object.
        /// </summary>
        Type Type { get; }
    }
}