using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly equal.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class NotEqualException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEqualException"/> class.
        /// </summary>
        public NotEqualException(string expected, string actual)
            : base("Not " + expected, actual, "Assert.NotEqual() Failure") { }
    }
}