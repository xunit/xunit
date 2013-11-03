using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly equal.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class NotEqualException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEqualException"/> class.
        /// </summary>
        public NotEqualException()
            : base("Assert.NotEqual() Failure") { }

        /// <inheritdoc/>
        protected NotEqualException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}