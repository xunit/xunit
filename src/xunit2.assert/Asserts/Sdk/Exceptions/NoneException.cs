using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when the collection did not contain exactly zero elements.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class NoneException : AssertCollectionCountException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoneException"/> class.
        /// </summary>
        /// <param name="count">The numbers of items in the collection.</param>
        public NoneException(int count) : base(0, count) { }
    }
}
