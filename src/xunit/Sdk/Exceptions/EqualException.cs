using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly not equal.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class EqualException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        public EqualException(object expected, object actual)
            : base(expected, actual, "Assert.Equal() Failure") { }

        /// <summary>
        /// Creates a new instance of the <see cref="EqualException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual object value</param>
        /// <param name="skipPositionCheck">Set to true to skip the check for difference position</param>
        public EqualException(object expected, object actual, bool skipPositionCheck)
            : base(expected, actual, "Assert.Equal() Failure", skipPositionCheck) { }

        /// <inheritdoc/>
        protected EqualException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}