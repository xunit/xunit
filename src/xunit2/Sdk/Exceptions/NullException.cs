using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an object reference is unexpectedly not null.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class NullException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NullException"/> class.
        /// </summary>
        /// <param name="actual"></param>
        public NullException(object actual)
            : base(null, actual, "Assert.Null() Failure") { }

        /// <inheritdoc/>
        protected NullException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}