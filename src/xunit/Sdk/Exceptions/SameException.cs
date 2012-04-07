using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two object references are unexpectedly not the same instance.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class SameException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SameException"/> class.
        /// </summary>
        /// <param name="expected">The expected object reference</param>
        /// <param name="actual">The actual object reference</param>
        public SameException(object expected,
                             object actual)
            : base(expected, actual, "Assert.Same() Failure", true) { }

        /// <inheritdoc/>
        protected SameException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}