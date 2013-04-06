using System;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents a reflection-backed implementation of <see cref="IAttributeInfo"/>.
    /// </summary>
    public interface IReflectionAttributeInfo : IAttributeInfo
    {
        /// <summary>
        /// Gets the instance of the attribute, if available.
        /// </summary>
        Attribute Attribute { get; }
    }
}