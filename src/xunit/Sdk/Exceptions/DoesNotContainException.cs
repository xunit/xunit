using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly contains the expected value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class DoesNotContainException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DoesNotContainException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        public DoesNotContainException(object expected)
            : base(String.Format(CultureInfo.CurrentCulture, "Assert.DoesNotContain() failure: Found: {0}", expected)) { }

        /// <inheritdoc/>
        protected DoesNotContainException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}