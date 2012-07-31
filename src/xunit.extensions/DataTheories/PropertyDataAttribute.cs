using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Xunit.Extensions
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from a public static property on the test class.
    /// The property must return IEnumerable&lt;object[]&gt; with the test data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class PropertyDataAttribute : DataAttribute
    {
        readonly string propertyName;

        /// <summary>
        /// Creates a new instance of <see cref="PropertyDataAttribute"/>/
        /// </summary>
        /// <param name="propertyName">The name of the public static property on the test class that will provide the test data</param>
        public PropertyDataAttribute(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName
        {
            get { return propertyName; }
        }

        /// <summary>
        /// Gets or sets the type to retrieve the property data from. If not set, then the property will be
        /// retrieved from the unit test class.
        /// </summary>
        public Type PropertyType { get; set; }

        /// <summary>
        /// Returns the data to be used to test the theory.
        /// </summary>
        /// <param name="methodUnderTest">The method that is being tested</param>
        /// <param name="parameterTypes">The types of the parameters for the test method</param>
        /// <returns>The theory data, in table form</returns>
        public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
        {
            Guard.ArgumentNotNull("methodUnderTest", methodUnderTest);

            Type type = PropertyType ?? methodUnderTest.DeclaringType;
            PropertyInfo propInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (propInfo == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Could not find public static property {0} on {1}", propertyName, type.FullName));

            object obj = propInfo.GetValue(null, null);
            if (obj == null)
                return null;

            IEnumerable<object[]> dataItems = obj as IEnumerable<object[]>;
            if (dataItems == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Property {0} on {1} did not return IEnumerable<object[]>", propertyName, type.FullName));

            return dataItems;
        }
    }
}