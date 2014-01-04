using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly contains the expected value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class DoesNotContainException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotContainException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        public DoesNotContainException(object expected)
            : base(String.Format(CultureInfo.CurrentCulture, "Assert.DoesNotContain() Failure: Found: {0}", expected)) { }
    }
}