using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly true.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class FalseException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FalseException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be display, or null for the default message</param>
        public FalseException(string userMessage)
            : base(userMessage ?? "Assert.False() Failure") { }

        /// <inheritdoc/>
        protected FalseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}