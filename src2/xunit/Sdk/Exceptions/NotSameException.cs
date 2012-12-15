using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpected the same instance.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class NotSameException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotSameException"/> class.
        /// </summary>
        public NotSameException()
            : base("Assert.NotSame() Failure") { }

        /// <inheritdoc/>
        protected NotSameException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}