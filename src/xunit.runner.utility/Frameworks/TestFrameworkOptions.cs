using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents options passed to a test framework for discovery or execution.
    /// </summary>
    public class TestFrameworkOptions : LongLivedMarshalByRefObject, ITestFrameworkOptions
    {
        readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        /// <summary>
        /// Gets a value from the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="defaultValue">The default value to use if the value is not present.</param>
        /// <returns>Returns the value.</returns>
        public TValue GetValue<TValue>(string name, TValue defaultValue)
        {
            object result;
            if (properties.TryGetValue(name, out result))
                return (TValue)result;

            return defaultValue;
        }

        /// <summary>
        /// Sets a value into the options collection.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the value.</param>
        /// <param name="value">The value.</param>
        public void SetValue<TValue>(string name, TValue value)
        {
            properties[name] = value;
        }
    }
}
