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
        private readonly IDictionary<string, XunitSerializationTriple> data = new Dictionary<string, XunitSerializationTriple>();

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

            data[key] = new XunitSerializationTriple(key, value, type);
        }

        /// <inheritdoc/>
        public T GetValue<T>(string key)
        {
            return (T)GetValue(key, typeof(T));
        }

        /// <inheritdoc/>
        public object GetValue(string key, Type type)
        {
            XunitSerializationTriple val;

            if (data.TryGetValue(key, out val))
                return val.Value;

            if (type.IsValueType())
                return Activator.CreateInstance(type);

            return null;
        }

        /// <summary>
        /// Returns BASE64 encoded string that represents the entirety of the data.
        /// </summary>
        public string ToSerializedString()
        {
            return ToBase64(String.Join("\n", data.Select(kvp => SerializeTriple(kvp.Value)).ToArray()));
        }

        /// <summary>
        /// Returns a triple for a key/value pair of data in a complex object
        /// </summary>
        /// <param name="triple">The triple to be serialized</param>
        /// <returns>The serialized version of the triple</returns>
        public static string SerializeTriple(XunitSerializationTriple triple)
        {
            var serializedType = SerializationHelper.GetTypeNameForSerialization(triple.Type);
            var serializedValue = Serialize(triple.Value);
            // Leaving off the colon is how we indicate null-ness
            if (serializedValue == null)
                return String.Format("{0}:{1}", triple.Key, serializedType);

            return String.Format("{0}:{1}:{2}", triple.Key, serializedType, serializedValue);
        }

        /// <summary>
        /// Returns the triple values out of a serialized triple.
        /// </summary>
        /// <param name="value">The serialized triple</param>
        /// <returns>The de-serialized triple</returns>
        public static XunitSerializationTriple DeserializeTriple(string value)
        {
            var pieces = value.Split(new[] { ':' }, 3);
            if (pieces.Length < 2)
                throw new ArgumentException("Data does not appear to be a valid serialized triple: " + value);

            var pieceType = SerializationHelper.GetType(pieces[1]);
            var deserializedValue = pieces.Length == 3 ? Deserialize(pieceType, pieces[2]) : null;

            return new XunitSerializationTriple(pieces[0], deserializedValue, pieceType);
        }

        /// <summary>
        /// De-serializes a value that was serialized with <see cref="XunitSerializationInfo"/>.
        /// </summary>
        /// <param name="type">The type of the object to de-serialize into</param>
        /// <param name="serializedValue">The serialized value</param>
        /// <returns>The de-serialized object</returns>
        public static object Deserialize(Type type, string serializedValue)
        {
            if (serializedValue == null)
                return null;

            if (typeof(IXunitSerializable).IsAssignableFrom(type))
                return DeserializeSerializable(type, serializedValue);

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

            if (type == typeof(bool?) || type == typeof(bool))
                return Boolean.Parse(serializedValue);

            if (type == typeof(DateTime?) || type == typeof(DateTime))
            {
                var styles = serializedValue.EndsWith("Z") ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
                return DateTime.Parse(serializedValue, CultureInfo.InvariantCulture, styles);
            }

            if (type == typeof(DateTimeOffset?) || type == typeof(DateTimeOffset))
            {
                var styles = serializedValue.EndsWith("Z") ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
                return DateTimeOffset.Parse(serializedValue, CultureInfo.InvariantCulture, styles);
            }

            if (type.IsEnum() || type.IsNullableEnum())
                return Enum.Parse(type.UnwrapNullable(), serializedValue);

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
                var triple = DeserializeTriple(element);
                serializationInfo.data[triple.Key] = triple;
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
                return null;

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

            var booleanData = value as bool?;
            if (booleanData != null)
                return booleanData.GetValueOrDefault().ToString();

            var datetimeData = value as DateTime?;
            if (datetimeData != null)
                return datetimeData.GetValueOrDefault().ToString("o", CultureInfo.InvariantCulture);  // Round-trippable format

            var datetimeoffsetData = value as DateTimeOffset?;
            if (datetimeoffsetData != null)
                return datetimeoffsetData.GetValueOrDefault().ToString("o", CultureInfo.InvariantCulture);  // Round-trippable format

            if (value.GetType().IsEnum())
                return value.ToString();

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
            typeof(int),            typeof(int?),
            typeof(long),           typeof(long?),
            typeof(float),          typeof(float?),
            typeof(double),         typeof(double?),
            typeof(decimal),        typeof(decimal?),
            typeof(bool),           typeof(bool?),
            typeof(DateTime),       typeof(DateTime?),
            typeof(DateTimeOffset), typeof(DateTimeOffset?),
        };

        private static bool CanSerializeObject(object value)
        {
            if (value == null)
                return true;

            var valueType = value.GetType();
            if (valueType.IsArray)
                return ((object[])value).All(CanSerializeObject);

            if (valueType.IsEnum() || valueType.IsNullableEnum())
                return true;

            if (supportedSerializationTypes.Any(supportedType => supportedType.IsAssignableFrom(valueType)))
                return true;

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
    }

    /// <summary>
    /// Represents a triple of information used when serializing complex types: the property name,
    /// the value to be serialized, and the value's type.
    /// </summary>
    internal class XunitSerializationTriple
    {
        /// <summary>
        /// Gets the triple's key
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Gets the triple's value
        /// </summary>
        public readonly object Value;

        /// <summary>
        /// Gets the triple's value type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitSerializationTriple"/> class.
        /// </summary>
        /// <param name="key">The triple's key</param>
        /// <param name="value">The triple's value</param>
        /// <param name="type">The triple's value type</param>
        public XunitSerializationTriple(string key, object value, Type type)
        {
            Key = key;
            Value = value;
            Type = type;
        }
    }
}