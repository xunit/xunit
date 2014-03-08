using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from a public static property on the test class.
    /// The property must return IEnumerable&lt;object[]&gt; with the test data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class PropertyDataAttribute : DataAttribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="PropertyDataAttribute"/>/
        /// </summary>
        /// <param name="propertyName">The name of the public static property on the test class that will provide the test data</param>
        public PropertyDataAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the type to retrieve the property data from. If not set, then the property will be
        /// retrieved from the unit test class.
        /// </summary>
        public Type PropertyType { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            Guard.ArgumentNotNull("methodUnderTest", testMethod);

            var type = PropertyType ?? testMethod.DeclaringType;
            PropertyInfo propInfo = null;

            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
            {
                propInfo = reflectionType.GetRuntimeProperty(PropertyName);
                if (propInfo != null)
                    break;
            }

            if (propInfo == null || propInfo.GetMethod == null || !propInfo.GetMethod.IsStatic)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Could not find public static get property {0} on {1}", PropertyName, type.FullName));

            object obj = propInfo.GetValue(null, null);
            if (obj == null)
                return null;

            var dataItems = obj as IEnumerable<object[]>;
            if (dataItems == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Property {0} on {1} did not return IEnumerable<object[]>", PropertyName, type.FullName));

            return dataItems;
        }
    }
}