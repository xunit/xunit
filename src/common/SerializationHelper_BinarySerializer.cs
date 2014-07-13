using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Serializes and de-serializes <see cref="ITestCase"/> instances using <see cref="BinaryFormatter"/>,
    /// <see cref="Convert.ToBase64String(byte[])"/>, and <see cref="Convert.FromBase64String"/>.
    /// </summary>
    public static class SerializationHelper
    {
        static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        /// <summary>
        /// De-serializes an object.
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="serializedValue">The object's serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static T Deserialize<T>(string serializedValue)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(serializedValue)))
                return (T)BinaryFormatter.Deserialize(stream);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The serialized value</returns>
        public static string Serialize(object value)
        {
            using (var stream = new MemoryStream())
            {
                BinaryFormatter.Serialize(stream, value);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }
    }
}
