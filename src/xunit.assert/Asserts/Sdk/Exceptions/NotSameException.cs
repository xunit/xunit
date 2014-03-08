using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpected the same instance.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class NotSameException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotSameException"/> class.
        /// </summary>
        public NotSameException()
            : base("Assert.NotSame() Failure") { }
    }
}