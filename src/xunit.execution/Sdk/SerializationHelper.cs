
using System;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Xunit.Abstractions;

#if !JSON
using System.Runtime.Serialization.Formatters.Binary;
#else

using Newtonsoft.Json.Serialization;
#endif

namespace Xunit.Sdk
{

#if !JSON
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
    using Newtonsoft.Json;
    internal static class SerializationHelper
    {
        private static readonly JsonSerializerSettings SerializerSettings = GetSerializerSettings();

        private static JsonSerializerSettings GetSerializerSettings()
        {
            var ser = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            return ser;
        }

        public static string Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException("value");

            var getData = value as IGetTypeData;
            if (getData != null)
            {

                var h = Xunit.Serialization.SerializationInfo.ObjectToSerializationInfo(getData);

                return ToJson(h);
            }

            return ToJson(value);
        }

        


        public static T Deserialize<T>(string serializedValue)
        {
            return (T)FromJson(serializedValue);
        }

         internal static string ToJson(object @object)
        {
            if (@object == null) throw new ArgumentNullException("object");


            return JsonConvert.SerializeObject(@object, Formatting.Indented, SerializerSettings);
        }

        internal static object FromJson(string json)
        {

            var info = JsonConvert.DeserializeObject(json, SerializerSettings);

            var ser = info as Xunit.Serialization.SerializationInfo;
            if (ser != null)
            {
                return Xunit.Serialization.SerializationInfo.CreateObjectFromSerializationInfo(ser);
            }


            return info;
        }
    }
#endif
}
