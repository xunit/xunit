using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when the collection did not contain exactly zero elements.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class NoneException : AssertCollectionCountException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoneException"/> class.
        /// </summary>
        /// <param name="count">The numbers of items in the collection.</param>
        public NoneException(int count) : base(0, count) { }

        /// <inheritdoc/>
        protected NoneException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
