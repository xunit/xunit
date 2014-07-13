using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Xunit.Serialization
{
    /// <summary>
    /// A mirror class of the CLR's <see cref="T:System.Runtime.Serialization.SerializationInfo"/> class.
    /// </summary>
    public class XunitSerializationInfo
    {
        private readonly IDictionary<string, Tuple<object, Type>> data = new Dictionary<string, Tuple<object, Type>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class.
        /// </summary>
        /// <param name="object">The data to copy into the serialization info</param>
        public XunitSerializationInfo(IGetTypeData @object = null)
        {
            if (@object != null)
                @object.GetData(this);
        }

        /// <summary>
        /// Adds a value to the collection.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <param name="type">The value's type</param>
        public void AddValue(string key, object value, Type type = null)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();

            data[key] = Tuple.Create(value, type);
        }

        /// <summary>
        /// Gets a string value from the collection.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The string value, if present; <c>null</c>, otherwise</returns>
        public string GetString(string key)
        {
            Tuple<object, Type> val;

            if (data.TryGetValue(key, out val))
                return (string)val.Item1;

            return null;
        }

        /// <summary>
        /// Gets a value from the collection.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="type">The value type</param>
        /// <returns>The string value, if present; <c>null</c>, otherwise</returns>
        public object GetValue(string key, Type type)
        {
            Tuple<object, Type> val;

            if (data.TryGetValue(key, out val))
                return val.Item1;

            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        /// <summary>
        /// Returns BASE64 encoded string that represents the entirety of the data.
        /// </summary>
        /// <returns></returns>
        public string ToSerializedString()
        {
            var valueTree = String.Join("\n", data.Select(kvp => String.Format("{0}:{1}:{2}", kvp.Key, kvp.Value.Item2.AssemblyQualifiedName, Serialize(kvp.Value.Item1))));
            return ToBase64(valueTree);
        }

        /// <summary>
        /// De-serializes a value that was serialized with <see cref="XunitSerializationInfo"/>.
        /// </summary>
        /// <param name="type">The type of the object to de-serialize into</param>
        /// <param name="serializedValue">The serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static object Deserialize(Type type, string serializedValue)
        {
            if (serializedValue == "")
                return null;

            if (typeof(IGetTypeData).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return DeserializeGetTypeData(type, serializedValue);

            if (type == typeof(string))
                return FromBase64(serializedValue);

            if (type == typeof(int?))
                return Int32.Parse(serializedValue);

            throw new ArgumentException("We don't know how to de-serialize type " + type.FullName, "serializedValue");
        }

        static IGetTypeData DeserializeGetTypeData(Type type, string serializedValue)
        {
            var serializationInfo = new XunitSerializationInfo();
            var elements = FromBase64(serializedValue).Split('\n');

            foreach (var element in elements)
            {
                var pieces = element.Split(new[] { ':' }, 3);
                if (pieces.Length != 3)
                    throw new ArgumentException("Could not split element into 3 pieces: " + element);

                var pieceType = Type.GetType(pieces[1]);
                serializationInfo.data[pieces[0]] = Tuple.Create(Deserialize(pieceType, pieces[2]), pieceType);
            }

            var value = (IGetTypeData)Activator.CreateInstance(type);
            value.SetData(serializationInfo);
            return value;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">The value to be serialized</param>
        /// <returns>The serialized object</returns>
        public static string Serialize(object value)
        {
            if (value == null)
                return "";

            var getTypeData = value as IGetTypeData;
            if (getTypeData != null)
            {
                var info = new XunitSerializationInfo();
                getTypeData.GetData(info);
                return info.ToSerializedString();
            }

            var stringData = value as string;
            if (stringData != null)
                return ToBase64(stringData);

            var nullableIntData = value as int?;
            if (nullableIntData != null)
                return nullableIntData.GetValueOrDefault().ToString();

            throw new ArgumentException("We don't know how to serialize type " + value.GetType().FullName, "value");
        }

        private static string FromBase64(string serializedValue)
        {
            var bytes = Convert.FromBase64String(serializedValue);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        private static string ToBase64(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }
    }
}