
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit.Abstractions;

#if !WINDOWS_PHONE_APP
using System.Runtime.Serialization.Formatters.Binary;
#endif
namespace Xunit.Sdk
{

#if !WINDOWS_PHONE_APP
    /// <summary>
    /// Serializes and de-serializes <see cref="ITestCase"/> instances using <see cref="BinaryFormatter"/>,
    /// <see cref="Convert.ToBase64String(byte[])"/>, and <see cref="Convert.FromBase64String"/>.
    /// </summary>
    public static class SerializationHelper
    {
        static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        /// <inheritdoc/>
        public static T Deserialize<T>(string serializedValue)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(serializedValue)))
                return (T)BinaryFormatter.Deserialize(stream);
        }

        /// <inheritdoc/>
        public static string Serialize(object value)
        {
            using (var stream = new MemoryStream())
            {
                BinaryFormatter.Serialize(stream, value);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
    }
#else
    internal static class SerializationHelper
    {
        public static string Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            
            return SerializationInfo.ToJson((ISerializable)value);
        }

        public static T Deserialize<T>(string serializedValue)
        {
            return (T)SerializationInfo.FromJson(serializedValue);
        }
    }
#endif
}
