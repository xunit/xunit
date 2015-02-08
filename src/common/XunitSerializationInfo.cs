using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Serialization
{
    /// <summary>
    /// A mirror class of the CLR's <see cref="T:System.Runtime.Serialization.SerializationInfo"/> class.
    /// </summary>
    internal class XunitSerializationInfo : IXunitSerializationInfo
    {
        private readonly IDictionary<string, SerializationData> data = new Dictionary<string, SerializationData>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class.
        /// </summary>
        /// <param name="object">The data to copy into the serialization info</param>
        public XunitSerializationInfo(IXunitSerializable @object = null)
        {
            if (@object != null)
                @object.Serialize(this);
        }

        /// <inheritdoc/>
        public void AddValue(string key, object value, Type type = null)
        {
            if (type == null)
                type = value == null ? typeof(object) : value.GetType();

            data[key] = new SerializationData(value, type);
        }

        /// <inheritdoc/>
        public T GetValue<T>(string key)
        {
            return (T)GetValue(key, typeof(T));
        }

        /// <inheritdoc/>
        public object GetValue(string key, Type type)
        {
            SerializationData val;

            if (data.TryGetValue(key, out val))
                return val.Value;

#if NEW_REFLECTION
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);
#else
            if (type.IsValueType)
                return Activator.CreateInstance(type);
#endif

            return null;
        }

        /// <summary>
        /// Returns BASE64 encoded string that represents the entirety of the data.
        /// </summary>
        /// <returns></returns>
        public string ToSerializedString()
        {
            var valueTree = String.Join("\n", data.Select(kvp => String.Format("{0}:{1}:{2}", kvp.Key, SerializationHelper.GetTypeNameForSerialization(kvp.Value.Type), Serialize(kvp.Value.Value))).ToArray());
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

#if NEW_REFLECTION
            if (typeof(IXunitSerializable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
                return DeserializeSerializable(type, serializedValue);
#else
            if (typeof(IXunitSerializable).IsAssignableFrom(type))
                return DeserializeSerializable(type, serializedValue);
#endif

            if (type == typeof(string))
                return FromBase64(serializedValue);

            if (type == typeof(int?) || type == typeof(int))
                return Int32.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(long?) || type == typeof(long))
                return Int64.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(float?) || type == typeof(float))
                return Single.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(double?) || type == typeof(double))
                return Double.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(decimal?) || type == typeof(decimal))
                return Decimal.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type.IsArray)
            {
                var arrSer = (ArraySerializer)DeserializeSerializable(typeof(ArraySerializer), serializedValue);
                return arrSer.ArrayData;
            }

            throw new ArgumentException("We don't know how to de-serialize type " + type.FullName, "serializedValue");
        }

        static IXunitSerializable DeserializeSerializable(Type type, string serializedValue)
        {
            var serializationInfo = new XunitSerializationInfo();
            var elements = FromBase64(serializedValue).Split('\n');

            foreach (var element in elements)
            {
                var pieces = element.Split(new[] { ':' }, 3);
                if (pieces.Length != 3)
                    throw new ArgumentException("Could not split element into 3 pieces: " + element);

                var pieceType = SerializationHelper.GetType(pieces[1]);
                serializationInfo.data[pieces[0]] = new SerializationData(Deserialize(pieceType, pieces[2]), pieceType);
            }

            var value = (IXunitSerializable)Activator.CreateInstance(type);
            value.Deserialize(serializationInfo);
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

            var serializable = value as IXunitSerializable;
            if (serializable != null)
            {
                var info = new XunitSerializationInfo();
                serializable.Serialize(info);
                return info.ToSerializedString();
            }

            var stringData = value as string;
            if (stringData != null)
                return ToBase64(stringData);

            var intData = value as int?;
            if (intData != null)
                return intData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var longData = value as long?;
            if (longData != null)
                return longData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var floatData = value as float?;
            if (floatData != null)
                return floatData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var doubleData = value as double?;
            if (doubleData != null)
                return doubleData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var decimalData = value as decimal?;
            if (decimalData != null)
                return decimalData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var array = value as object[];
            if (array != null)
            {
                var info = new XunitSerializationInfo();
                var arraySer = new ArraySerializer(array);
                arraySer.Serialize(info);
                return info.ToSerializedString();
            }

            throw new ArgumentException("We don't know how to serialize type " + value.GetType().FullName, "value");
        }

        static readonly Type[] supportedSerializationTypes = new[] {
            typeof(IXunitSerializable),
            typeof(string),
            typeof(int),     typeof(int?),
            typeof(long),    typeof(long?),
            typeof(float),   typeof(float?),
            typeof(double),  typeof(double?),
            typeof(decimal), typeof(decimal?),
        };

        private static bool CanSerializeObject(object value)
        {
            if (value == null)
                return true;

            var valueType = value.GetType();
            if (valueType.IsArray)
                return ((object[])value).All(CanSerializeObject);

#if NEW_REFLECTION
            var typeInfo = valueType.GetTypeInfo();
            if (supportedSerializationTypes.Any(supportedType => supportedType.GetTypeInfo().IsAssignableFrom(typeInfo)))
                return true;
#else
            if (supportedSerializationTypes.Any(supportedType => supportedType.IsAssignableFrom(valueType)))
                return true;
#endif

            return false;
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

        internal class ArraySerializer : IXunitSerializable
        {
            object[] array;
            readonly Type elementType;

            public object[] ArrayData { get { return array; } }

            public ArraySerializer() { }

            public ArraySerializer(object[] array)
            {
                if (array == null)
                    throw new ArgumentNullException("array");

                if (!CanSerializeObject(array))
                    throw new ArgumentException("There is at least one object in this array that cannot be serialized", "array");

                this.array = array;
                elementType = array.GetType().GetElementType();
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("Length", array.Length);
                info.AddValue("ElementType", elementType.FullName);

                for (var i = 0; i < array.Length; i++)
                    info.AddValue("Item" + i, array[i]);
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                var len = info.GetValue<int>("Length");
                var arrType = SerializationHelper.GetType(info.GetValue<string>("ElementType"));

                array = Array.CreateInstance(arrType, len) as object[];

                for (var i = 0; i < array.Length; i++)
                    array[i] = info.GetValue("Item" + i, arrType);
            }
        }

        class SerializationData
        {
            public object Value;
            public Type Type;

            public SerializationData(object value, Type type)
            {
                Value = value;
                Type = type;
            }
        }
    }
}