using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when code raises a property changed event before the property actually changed.
    /// </summary>
    public class PropertyChangedPrematurelyException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PropertyChangedPrematurelyException"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        public PropertyChangedPrematurelyException(string propertyName)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.PropertyChanged failure: Property {0} was not set to a new value before PropertyChanged was raised", propertyName))
        {
        }
    }
}