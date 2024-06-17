using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
using Xunit.Sdk;
#endif

#if !NETFRAMEWORK
using System.Numerics;
#endif

#if !NET35
using System.Reflection;
#endif

namespace Xunit.Serialization
{
    /// <summary>
    /// A mirror class of the CLR's <see cref="T:System.Runtime.Serialization.SerializationInfo"/> class.
    /// </summary>
    class XunitSerializationInfo : IXunitSerializationInfo
    {
        readonly IDictionary<string, XunitSerializationTriple> data = new Dictionary<string, XunitSerializationTriple>();

        // DateOnly and TimeOnly only exist in .NET 6+, so we must access them via reflection. There is no way for us to
        // do this from .NET 3.5 (and doesn't matter, because those types would never exist there) so we have to get all
        // this code away from .NET 3.5.
#if !NET35
        static readonly Type dateOnlyType = Type.GetType("System.DateOnly");
        static readonly Type dateOnlyNullableType = dateOnlyType == null ? null : typeof(Nullable<>).MakeGenericType(dateOnlyType);
        static readonly PropertyInfo dateOnlyDayNumber = dateOnlyType?.GetTypeInfo().GetDeclaredProperty("DayNumber");
        static readonly MethodInfo dateOnlyFromDayNumber = dateOnlyType?.GetTypeInfo().GetDeclaredMethods("FromDayNumber").FirstOrDefault(m => m.IsPublic && m.IsStatic && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int));

        static readonly Type timeOnlyType = Type.GetType("System.TimeOnly");
        static readonly Type timeOnlyNullableType = timeOnlyType == null ? null : typeof(Nullable<>).MakeGenericType(timeOnlyType);
        static readonly PropertyInfo timeOnlyTicks = timeOnlyType?.GetTypeInfo().GetDeclaredProperty("Ticks");
        static readonly ConstructorInfo timeOnlyCtor = timeOnlyType?.GetTypeInfo().DeclaredConstructors.FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(long));
#endif

        static readonly Dictionary<Type, bool> enumSignsByType = new()
        {
            // Signed
            { typeof(sbyte), true },
            { typeof(short), true },
            { typeof(int), true },
            { typeof(long), true },

            // Unsigned
            { typeof(byte), false },
            { typeof(ushort), false },
            { typeof(uint), false },
            { typeof(ulong), false },
        };

        static List<Type> supportedSerializationTypes = new()
        {
            typeof(IXunitSerializable),
            typeof(char),
            typeof(char?),
            typeof(string),
            typeof(byte),
            typeof(byte?),
            typeof(sbyte),
            typeof(sbyte?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?),
            typeof(bool),
            typeof(bool?),
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
#if !NETFRAMEWORK
            typeof(BigInteger),
            typeof(BigInteger?),
#endif
            typeof(TimeSpan),
            typeof(TimeSpan?),
        };

#if !NET35
        static XunitSerializationInfo()
        {
            if (dateOnlyType != null)
            {
                supportedSerializationTypes.Add(dateOnlyType);
                supportedSerializationTypes.Add(dateOnlyNullableType);
            }

            if (timeOnlyType != null)
            {
                supportedSerializationTypes.Add(timeOnlyType);
                supportedSerializationTypes.Add(timeOnlyNullableType);
            }
        }
#endif

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

        internal static bool CanSerializeObject(object value)
        {
            if (value == null)
                return true;

            var valueType = value.GetType();

            if (valueType.IsArray)
            {
                var vector = value as object[];
                if (vector != null)
                {
                    // Avoid enumerator allocation and bounds lookups that comes from enumerating a System.Array
                    foreach (object obj in vector)
                        if (!CanSerializeObject(obj))
                            return false;
                }
                else
                {
                    foreach (object obj in ((Array)value))
                        if (!CanSerializeObject(obj))
                            return false;
                }
                return true;
            }

            foreach (Type supportedType in supportedSerializationTypes)
                if (supportedType.IsAssignableFrom(valueType))
                    return true;

            Type typeToCheck = valueType;
            if (valueType.IsEnum() || valueType.IsNullableEnum() || (typeToCheck = value as Type) != null)
                return typeToCheck.FullName != null && typeToCheck.IsFromLocalAssembly();

            return false;
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

            if (type == typeof(char?) || type == typeof(char))
                return (char)ushort.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(string))
                return FromBase64(serializedValue);

            if (type == typeof(byte?) || type == typeof(byte))
                return byte.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(sbyte?) || type == typeof(sbyte))
                return sbyte.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(short?) || type == typeof(short))
                return short.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(ushort?) || type == typeof(ushort))
                return ushort.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(int?) || type == typeof(int))
                return int.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(uint?) || type == typeof(uint))
                return uint.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(long?) || type == typeof(long))
                return long.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(ulong?) || type == typeof(ulong))
                return ulong.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(float?) || type == typeof(float))
            {
                var arrSer = (ArraySerializer)DeserializeSerializable(typeof(ArraySerializer), serializedValue);
                byte[] bytes = (byte[])arrSer.ArrayData;
                return BitConverter.ToSingle(bytes, 0);
            }

            if (type == typeof(double?) || type == typeof(double))
            {
                var arrSer = (ArraySerializer)DeserializeSerializable(typeof(ArraySerializer), serializedValue);
                byte[] bytes = (byte[])arrSer.ArrayData;
                return BitConverter.ToDouble(bytes, 0);
            }

            if (type == typeof(decimal?) || type == typeof(decimal))
                return decimal.Parse(serializedValue, CultureInfo.InvariantCulture);

            if (type == typeof(bool?) || type == typeof(bool))
                return bool.Parse(serializedValue);

            if (type == typeof(DateTime?) || type == typeof(DateTime))
            {
                var styles = serializedValue.EndsWith("Z", StringComparison.Ordinal) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
                return DateTime.Parse(serializedValue, CultureInfo.InvariantCulture, styles);
            }

            if (type == typeof(DateTimeOffset?) || type == typeof(DateTimeOffset))
            {
                var styles = serializedValue.EndsWith("Z", StringComparison.Ordinal) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;
                return DateTimeOffset.Parse(serializedValue, CultureInfo.InvariantCulture, styles);
            }

#if !NET35
            if ((type == dateOnlyType || type == dateOnlyNullableType) && dateOnlyFromDayNumber != null)
                return dateOnlyFromDayNumber.Invoke(null, new object[] { int.Parse(serializedValue, CultureInfo.InvariantCulture) });

            if ((type == timeOnlyType || type == timeOnlyNullableType) && timeOnlyCtor != null)
                return timeOnlyCtor.Invoke(new object[] { long.Parse(serializedValue, CultureInfo.InvariantCulture) });

            if (type == typeof(TimeSpan?) || type == typeof(TimeSpan))
                return TimeSpan.Parse(serializedValue, CultureInfo.InvariantCulture);
#endif

#if !NETFRAMEWORK
            if (type == typeof(BigInteger?) || type == typeof(BigInteger))
                return BigInteger.Parse(serializedValue, CultureInfo.InvariantCulture);
#endif

            if (type == typeof(Type))
                return SerializationHelper.GetType(serializedValue);

            if (type.IsEnum() || type.IsNullableEnum())
                return Enum.Parse(type.UnwrapNullable(), serializedValue);

            if (type.IsArray)
            {
                var arrSer = (ArraySerializer)DeserializeSerializable(typeof(ArraySerializer), serializedValue);
                return arrSer.ArrayData;
            }

            throw new ArgumentException("We don't know how to de-serialize type " + type.FullName, nameof(serializedValue));
        }

        static IXunitSerializable DeserializeSerializable(Type type, string serializedValue)
        {
            var serializationInfo = new XunitSerializationInfo();

            // Will end up with an empty string if the serialization type did not serialize any data
            if (serializedValue != string.Empty)
            {
                var elements = FromBase64(serializedValue).Split('\n');

                foreach (var element in elements)
                {
                    var triple = DeserializeTriple(element);
                    serializationInfo.data[triple.Key] = triple;
                }
            }

            try
            {
                var value = (IXunitSerializable)Activator.CreateInstance(type);
                value.Deserialize(serializationInfo);
                return value;
            }
            catch (MissingMemberException)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not de-serialize type '{0}' because it lacks a parameterless constructor.", type.FullName));
            }
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

        static string FromBase64(string serializedValue)
        {
            var bytes = Convert.FromBase64String(serializedValue);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
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

            var charData = value as char?;
            if (charData != null)
                return ((ushort)charData.GetValueOrDefault()).ToString(CultureInfo.InvariantCulture);

            var stringData = value as string;
            if (stringData != null)
                return ToBase64(stringData);

            var byteData = value as byte?;
            if (byteData != null)
                return byteData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var sbyteData = value as sbyte?;
            if (sbyteData != null)
                return sbyteData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var ushortData = value as ushort?;
            if (ushortData != null)
                return ushortData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var shortData = value as short?;
            if (shortData != null)
                return shortData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var uintData = value as uint?;
            if (uintData != null)
                return uintData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var intData = value as int?;
            if (intData != null)
                return intData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var ulongData = value as ulong?;
            if (ulongData != null)
                return ulongData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var longData = value as long?;
            if (longData != null)
                return longData.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);

            var floatData = value as float?;
            if (floatData != null)
            {
                var info = new XunitSerializationInfo();
                var arraySer = new ArraySerializer(BitConverter.GetBytes(floatData.GetValueOrDefault()));
                arraySer.Serialize(info);
                return info.ToSerializedString();
            }

            var doubleData = value as double?;
            if (doubleData != null)
            {
                var info = new XunitSerializationInfo();
                var arraySer = new ArraySerializer(BitConverter.GetBytes(doubleData.GetValueOrDefault()));
                arraySer.Serialize(info);
                return info.ToSerializedString();
            }

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

            var valueType = value.GetType();

#if !NET35
            if (valueType == dateOnlyType && dateOnlyDayNumber != null)
                return (dateOnlyDayNumber.GetValue(value) as int?)?.ToString(CultureInfo.InvariantCulture)
                    ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call DayNumber on an instance of '{0}': {1}", dateOnlyType.FullName, value));

            if (valueType == timeOnlyType && timeOnlyTicks != null)
                return (timeOnlyTicks.GetValue(value) as long?)?.ToString(CultureInfo.InvariantCulture)
                    ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call Ticks on an instance of '{0}': {1}", timeOnlyType.FullName, value));

            if (value is TimeSpan)
                return ((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture);
#endif

#if !NETFRAMEWORK
            if (value is BigInteger)
                return ((BigInteger)value).ToString(CultureInfo.InvariantCulture);
#endif

            var typeData = value as Type;
            if (typeData != null)
            {
                if (typeData.FullName == null)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "We don't know how to serialize value typeof({0}) (no full name)", typeData.Name), nameof(value));

                return SerializationHelper.GetTypeNameForSerialization(typeData);
            }

            if (valueType.IsEnum())
                return SerializeEnum(value, valueType);

            var arrayData = value as Array;
            if (arrayData != null)
            {
                var info = new XunitSerializationInfo();
                var arraySer = new ArraySerializer(arrayData);
                arraySer.Serialize(info);
                return info.ToSerializedString();
            }

            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "We don't know how to serialize type {0}", valueType.FullName), nameof(value));
        }

        static string SerializeEnum(object value, Type valueType)
        {
            if (!valueType.IsFromLocalAssembly())
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum {0}.{1} because it lives in the GAC", valueType.FullName, value), nameof(value));

            Type underlyingType;

            try
            {
                underlyingType = Enum.GetUnderlyingType(value.GetType());
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because an exception was thrown getting its underlying type", valueType.FullName, value), ex);
            }

            if (!enumSignsByType.TryGetValue(underlyingType, out var signed))
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because the underlying type '{2}' is not supported", valueType.FullName, value, underlyingType.FullName), nameof(value));

            return
                signed
                    ? Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
                    : Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
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
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", triple.Key, serializedType);

            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}:{2}", triple.Key, serializedType, serializedValue);
        }

        static string ToBase64(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Returns BASE64 encoded string that represents the entirety of the data.
        /// </summary>
        public string ToSerializedString()
        {
            return ToBase64(string.Join("\n", data.Select(kvp => SerializeTriple(kvp.Value)).ToArray()));
        }

        internal class ArraySerializer : IXunitSerializable
        {
            Array array;
            readonly Type elementType;

            public Array ArrayData { get { return array; } }

            public ArraySerializer() { }

            public ArraySerializer(Array array)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));

                if (!CanSerializeObject(array))
                    throw new ArgumentException("There is at least one object in this array that cannot be serialized", nameof(array));

                this.array = array;
                elementType = array.GetType().GetElementType();
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue("ElementType", SerializationHelper.GetTypeNameForSerialization(elementType));
                info.AddValue("Rank", array.Rank);
                info.AddValue("TotalLength", array.Length);

                for (int dimension = 0; dimension < array.Rank; dimension++)
                {
                    info.AddValue("Length" + dimension, array.GetLength(dimension));
                }
                for (int dimension = 0; dimension < array.Rank; dimension++)
                {
                    info.AddValue("LowerBound" + dimension, array.GetLowerBound(dimension));
                }

                int i = 0;
                foreach (object obj in array)
                {
                    info.AddValue("Item" + i, obj);
                    i++;
                }
            }

            public void Deserialize(IXunitSerializationInfo info)
            {
                var arrType = SerializationHelper.GetType(info.GetValue<string>("ElementType"));
                var rank = info.GetValue<int>("Rank");
                var totalLength = info.GetValue<int>("TotalLength");

                int[] lengths = new int[rank];
                int[] lowerBounds = new int[rank];
                for (int i = 0; i < lengths.Length; i++)
                {
                    lengths[i] = info.GetValue<int>("Length" + i);
                    lowerBounds[i] = info.GetValue<int>("LowerBound" + i);
                }

                array = Array.CreateInstance(arrType, lengths, lowerBounds);

                int[] indices = new int[rank];
                for (int i = 0; i < indices.Length; i++)
                {
                    indices[i] = lowerBounds[i];
                }
                for (int i = 0; i < totalLength; i++)
                {
                    bool complete = false;
                    for (int dim = rank - 1; dim >= 0; dim--)
                    {
                        if (indices[dim] >= lowerBounds[dim] + lengths[dim])
                        {
                            if (dim == 0)
                            {
                                complete = true;
                                break;
                            }
                            for (int j = dim; j < rank; j++)
                                indices[j] = lowerBounds[dim];
                            indices[dim - 1]++;
                        }
                    }
                    if (complete)
                    {
                        break;
                    }
                    object item = info.GetValue("Item" + i, arrType);
                    array.SetValue(item, indices);
                    indices[rank - 1]++;
                }
            }
        }
    }

    /// <summary>
    /// Represents a triple of information used when serializing complex types: the property name,
    /// the value to be serialized, and the value's type.
    /// </summary>
    class XunitSerializationTriple
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
