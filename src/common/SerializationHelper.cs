using System;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Serialization;

namespace Xunit.Sdk
{
    /// <summary>
    /// Serializes and de-serializes objects
    /// </summary>
    internal static class SerializationHelper
    {
        /// <summary>
        /// De-serializes an object.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="serializedValue">The object's serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static T Deserialize<T>(string serializedValue)
        {
            if (serializedValue == null)
                throw new ArgumentNullException("serializedValue");

            var pieces = serializedValue.Split(new[] { ':' }, 2);
            if (pieces.Length != 2)
                throw new ArgumentException("De-serialized string is in the incorrect format.");

            var deserializedType = Type.GetType(pieces[0]);
            if (deserializedType == null)
                throw new ArgumentException("Could not load type " + pieces[0], "serializedValue");

#if NEW_REFLECTION
            if (!typeof(IXunitSerializable).GetTypeInfo().IsAssignableFrom(deserializedType.GetTypeInfo()))
                throw new ArgumentException("Cannot de-serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "T");
#else
            if (!typeof(IXunitSerializable).IsAssignableFrom(deserializedType))
                throw new ArgumentException("Cannot de-serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "T");
#endif

            var obj = XunitSerializationInfo.Deserialize(deserializedType, pieces[1]);
            if (obj is XunitSerializationInfo.ArraySerializer)
                obj = ((XunitSerializationInfo.ArraySerializer)obj).ArrayData;

            return (T)obj;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value</returns>
        public static string Serialize(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var array = value as object[];
            if (array != null)
                value = new XunitSerializationInfo.ArraySerializer(array);

            var serializable = value as IXunitSerializable;
            if (serializable == null)
                throw new ArgumentException("Cannot serialize an object that does not implement " + typeof(IXunitSerializable).FullName, "value");

            var serializationInfo = new XunitSerializationInfo(serializable);
            return String.Format("{0}:{1}", value.GetType().AssemblyQualifiedName, serializationInfo.ToSerializedString());
        }
    }
}
