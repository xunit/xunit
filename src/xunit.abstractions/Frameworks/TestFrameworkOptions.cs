using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents options given to an implementation of <see cref="ITestFrameworkDiscoverer"/>.Find
    /// or <see cref="ITestFrameworkExecutor"/>.Run.
    /// </summary>
    [Serializable]
    public class TestFrameworkOptions : ISerializable
    {
        readonly Dictionary<string, KeyValuePair<Type, object>> properties = new Dictionary<string, KeyValuePair<Type, object>>();

        public TestFrameworkOptions() { }

        protected TestFrameworkOptions(SerializationInfo info, StreamingContext context)
        {
            foreach (var serializationEntry in info)
                properties[serializationEntry.Name] = new KeyValuePair<Type, object>(serializationEntry.ObjectType, serializationEntry.Value);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var kvp in properties)
                info.AddValue(kvp.Key, kvp.Value.Value, kvp.Value.Key);
        }

        public TValue GetValue<TValue>(string name, TValue defaultValue)
        {
            KeyValuePair<Type, object> result;
            if (properties.TryGetValue(name, out result))
                return (TValue)result.Value;

            return defaultValue;
        }

        public void SetValue<TValue>(string name, TValue value)
        {
            properties[name] = new KeyValuePair<Type, object>(typeof(TValue), value);
        }
    }
}
