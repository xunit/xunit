using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection unexpectedly does not contain the expected value.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class ContainsException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        public ContainsException(object expected)
            : base(string.Format("Assert.Contains() failure: Not found: {0}", expected)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="ContainsException"/> class.
        /// </summary>
        /// <param name="expected">The expected object value</param>
        /// <param name="actual">The actual value</param>
        public ContainsException(object expected, object actual)
            : base(string.Format("Assert.Contains() failure:{2}Not found: {0}{2}In value:  {1}", expected, actual, Environment.NewLine)) { }

        /// <inheritdoc/>
        protected ContainsException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}