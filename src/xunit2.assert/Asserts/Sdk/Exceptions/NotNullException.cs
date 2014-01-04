using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when an object is unexpectedly null.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class NotNullException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotNullException"/> class.
        /// </summary>
        public NotNullException()
            : base("Assert.NotNull() Failure") { }
    }
}