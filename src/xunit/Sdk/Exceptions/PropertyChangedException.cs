using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when code unexpectedly fails change a property.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [Serializable]
    public class PropertyChangedException : AssertException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PropertyChangedException"/> class. Call this constructor
        /// when no exception was thrown.
        /// </summary>
        /// <param name="propertyName">The name of the property that was expected to be changed.</param>
        public PropertyChangedException(string propertyName)
            : base(String.Format("Assert.PropertyChanged failure: Property {0} was not set", propertyName)) { }

        /// <inheritdoc/>
        protected PropertyChangedException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
