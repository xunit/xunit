using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly false.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class TrueException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TrueException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed, or null for the default message</param>
        public TrueException(string userMessage)
            : base(userMessage ?? "Assert.True() Failure") { }
    }
}