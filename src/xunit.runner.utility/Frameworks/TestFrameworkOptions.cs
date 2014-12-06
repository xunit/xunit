using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Represents options passed to a test framework for discovery or execution.
    /// </summary>
    public class TestFrameworkOptions : LongLivedMarshalByRefObject, ITestFrameworkOptions
    {
        readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        static bool IsEnum(Type type)
        {
#if NEW_REFLECTION
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

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
            {
                if (IsEnum(typeof(TValue)))
                    return (TValue)Enum.Parse(typeof(TValue), (string)result);
                else
                    return (TValue)result;
            }

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
            if (IsEnum(typeof(TValue)))
                properties[name] = value.ToString();
            else
                properties[name] = value;
        }
    }
}
