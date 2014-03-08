using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly true.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class FalseException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FalseException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be display, or null for the default message</param>
        public FalseException(string userMessage)
            : base(userMessage ?? "Assert.False() Failure") { }
    }
}