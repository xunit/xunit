using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an object is unexpectedly null.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class NotNullException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotNullException"/> class.
        /// </summary>
        public NotNullException()
            : base("Assert.NotNull() Failure") { }

        /// <inheritdoc/>
        protected NotNullException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}