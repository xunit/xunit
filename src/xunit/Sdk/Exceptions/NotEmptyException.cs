using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection is unexpectedly empty.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class NotEmptyException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEmptyException"/> class.
        /// </summary>
        public NotEmptyException()
            : base("Assert.NotEmpty() failure") { }

        /// <inheritdoc/>
        protected NotEmptyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}