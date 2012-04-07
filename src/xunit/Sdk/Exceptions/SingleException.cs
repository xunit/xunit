using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when the collection did not contain exactly one element.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class SingleException : AssertException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleException"/> class.
        /// </summary>
        /// <param name="count">The numbers of items in the collection.</param>
        public SingleException(int count)
            : base(String.Format("The collection contained {0} elements instead of 1.", count)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleException"/> class.
        /// </summary>
        /// <param name="count">The numbers of items in the collection.</param>
        /// <param name="expected">The object expected to be in the collection.</param>
        public SingleException(int count, object expected)
            : base(String.Format("The collection contained {0} instances of '{1}' instead of 1.", count, expected)) { }

        /// <inheritdoc/>
        protected SingleException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
