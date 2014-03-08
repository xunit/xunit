using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly does not contain the expected value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ContainsException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        public ContainsException(object expected)
            : base(String.Format(CultureInfo.CurrentCulture, "Assert.Contains() Failure: Not found: {0}", expected)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual value</param>
        public ContainsException(object expected, object actual)
            : base(String.Format(CultureInfo.CurrentCulture, "Assert.Contains() Failure:{2}Not found: {0}{2}In value:  {1}", expected, actual, Environment.NewLine)) { }
    }
}