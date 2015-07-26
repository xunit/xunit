using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when code unexpectedly fails change a property.
    /// </summary>
    public class PropertyChangedException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PropertyChangedException"/> class. Call this constructor
        /// when no exception was thrown.
        /// </summary>
        /// <param name="propertyName">The name of the property that was expected to be changed.</param>
        public PropertyChangedException(string propertyName)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.PropertyChanged failure: Property {0} was not set", propertyName))
        { }
    }
}
