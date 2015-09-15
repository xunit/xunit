using System.Globalization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when code raises a property changed event before the property actually changed.
    /// </summary>
    public class InaccessiblePropertyException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PropertyChangedPrematurelyException"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        public InaccessiblePropertyException(string propertyName)
            : base(string.Format(CultureInfo.CurrentCulture, "Assert.PropertyChanged failure: Property {0} does not have a public getter", propertyName))
        {
        }
    }
}