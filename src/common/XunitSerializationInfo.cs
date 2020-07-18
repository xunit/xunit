#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Serialization
{
	/// <summary>
	/// A mirror class of the CLR's <see cref="T:System.Runtime.Serialization.SerializationInfo"/> class.
	/// </summary>
	class XunitSerializationInfo : IXunitSerializationInfo
	{
		readonly IDictionary<string, XunitSerializationTriple> data = new Dictionary<string, XunitSerializationTriple>();

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitSerializationInfo"/> class.
		/// </summary>
		/// <param name="object">The data to copy into the serialization info</param>
		public XunitSerializationInfo(IXunitSerializable? @object = null)
		{
			if (@object != null)
				@object.Serialize(this);
		}

		/// <inheritdoc/>
		public void AddValue(string key, object? value, Type? type = null)
		{
			Guard.ArgumentNotNull(nameof(key), key);

			if (type == null)
				type = value == null ? typeof(object) : value.GetType();

			data[key] = new XunitSerializationTriple(key, value, type);
		}

#nullable disable  // The original signature is not compatibility with nullable reference type support
		/// <inheritdoc/>
		public T GetValue<T>(string key)
		{
			Guard.ArgumentNotNull(nameof(key), key);

			return (T)GetValue(key, typeof(T));
		}
#nullable enable

		/// <inheritdoc/>
		public object? GetValue(string key, Type type)
		{
			Guard.ArgumentNotNull(nameof(key), key);
			Guard.ArgumentNotNull(nameof(type), type);

			if (data.TryGetValue(key, out var val))
				return val.Value;

			if (type.IsValueType)
				return Activator.CreateInstance(type);

			return null;
		}

		/// <summary>
		/// Returns BASE64 encoded string that represents the entirety of the data.
		/// </summary>
		public string ToSerializedString() =>
			ToBase64(string.Join("\n", data.Select(kvp => SerializeTriple(kvp.Value)).ToArray()));

		/// <summary>
		/// Returns a triple for a key/value pair of data in a complex object
		/// </summary>
		/// <param name="triple">The triple to be serialized</param>
		/// <returns>The serialized version of the triple</returns>
		public static string SerializeTriple(XunitSerializationTriple triple)
		{
			Guard.ArgumentNotNull(nameof(triple), triple);

			var serializedType = SerializationHelper.GetTypeNameForSerialization(triple.Type);
			var serializedValue = Serialize(triple.Value);

			// Leaving off the colon is how we indicate null-ness
			if (serializedValue == null)
				return $"{triple.Key}:{serializedType}";

			return $"{triple.Key}:{serializedType}:{serializedValue}";
		}

		/// <summary>
		/// Returns the triple values out of a serialized triple.
		/// </summary>
		/// <param name="value">The serialized triple</param>
		/// <returns>The de-serialized triple</returns>
		public static XunitSerializationTriple DeserializeTriple(string value)
		{
			Guard.ArgumentNotNull(nameof(value), value);

			var pieces = value.Split(new[] { ':' }, 3);
			if (pieces.Length < 2)
				throw new ArgumentException("Data does not appear to be a valid serialized triple: " + value);

			var pieceType = SerializationHelper.GetType(pieces[1]);
			if (pieceType == null)
				throw new ArgumentException("Data does not appear to be a valid serialized triple: " + value);

			var deserializedValue = pieces.Length == 3 ? Deserialize(pieceType, pieces[2]) : null;

			return new XunitSerializationTriple(pieces[0], deserializedValue, pieceType);
		}

		/// <summary>
		/// De-serializes a value that was serialized with <see cref="XunitSerializationInfo"/>.
		/// </summary>
		/// <param name="type">The type of the object to de-serialize into</param>
		/// <param name="serializedValue">The serialized value</param>
		/// <returns>The de-serialized object</returns>
		public static object? Deserialize(Type type, string? serializedValue)
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
				var bytes = (byte[])arrSer.ArrayData!;
				return BitConverter.ToSingle(bytes, 0);
			}

			if (type == typeof(double?) || type == typeof(double))
			{
				var arrSer = (ArraySerializer)DeserializeSerializable(typeof(ArraySerializer), serializedValue);
				var bytes = (byte[])arrSer.ArrayData!;
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

			if (type == typeof(Type))
				return SerializationHelper.GetType(serializedValue);

			if (type.IsEnum || type.IsNullableEnum())
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
				if (!(Activator.CreateInstance(type) is IXunitSerializable value))
					throw new ArgumentException($"Could not cast '{type.FullName}' to '{typeof(IXunitSerializable).FullName}' for deserialization");

				value.Deserialize(serializationInfo);
				return value;
			}
			catch (MissingMemberException)
			{
				throw new InvalidOperationException($"Could not de-serialize type '{type.FullName}' because it lacks a parameterless constructor.");
			}
		}

		/// <summary>
		/// Serializes an object.
		/// </summary>
		/// <param name="value">The value to be serialized</param>
		/// <returns>The serialized object</returns>
		public static string? Serialize(object? value)
		{
			if (value == null)
				return null;

			if (value is IXunitSerializable serializable)
			{
				var info = new XunitSerializationInfo();
				serializable.Serialize(info);
				return info.ToSerializedString();
			}

			var charData = value as char?;
			if (charData != null)
				return ((ushort)charData.GetValueOrDefault()).ToString(CultureInfo.InvariantCulture);

			if (value is string stringData)
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

			var typeData = value as Type;
			if (typeData != null)
				return SerializationHelper.GetTypeNameForSerialization(typeData);

			var valueType = value.GetType();
			if (valueType.IsEnum)
			{
				if (!valueType.IsFromLocalAssembly())
					throw new ArgumentException($"We cannot serialize enum {valueType.FullName}.{value} because it lives in the GAC", nameof(value));
				return value.ToString();
			}

			if (value is Array arrayData)
			{
				var info = new XunitSerializationInfo();
				var arraySer = new ArraySerializer(arrayData);
				arraySer.Serialize(info);
				return info.ToSerializedString();
			}

			throw new ArgumentException($"We don't know how to serialize type {valueType.FullName}", nameof(value));
		}

		static readonly Type[] supportedSerializationTypes = {
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
		};

		internal static bool CanSerializeObject(object? value)
		{
			if (value == null)
				return true;

			var valueType = value.GetType();

			if (valueType.IsArray)
			{
				if (value is object[] vector)
				{
					// Avoid enumerator allocation and bounds lookups that comes from enumerating a System.Array
					foreach (var obj in vector)
						if (!CanSerializeObject(obj))
							return false;
				}
				else
				{
					foreach (var obj in (Array)value)
						if (!CanSerializeObject(obj))
							return false;
				}
				return true;
			}

			foreach (var supportedType in supportedSerializationTypes)
				if (supportedType.IsAssignableFrom(valueType))
					return true;

			var typeToCheck = valueType;
			if (valueType.IsEnum || valueType.IsNullableEnum() || (typeToCheck = value as Type) != null)
				return typeToCheck.IsFromLocalAssembly();

			return false;
		}

		static string FromBase64(string serializedValue)
		{
			var bytes = Convert.FromBase64String(serializedValue);
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		static string ToBase64(string value)
		{
			var bytes = Encoding.UTF8.GetBytes(value);
			return Convert.ToBase64String(bytes);
		}

		internal class ArraySerializer : IXunitSerializable
		{
			readonly Type? elementType;

			public Array? ArrayData { get; private set; }

			public ArraySerializer() { }

			public ArraySerializer(Array array)
			{
				Guard.ArgumentNotNull(nameof(array), array);

				if (!CanSerializeObject(array))
					throw new ArgumentException("There is at least one object in this array that cannot be serialized", nameof(array));

				ArrayData = array;
				elementType = array.GetType().GetElementType();
			}

			public void Serialize(IXunitSerializationInfo info)
			{
				Guard.ArgumentNotNull(nameof(info), info);
				Guard.NotNull("Cannot serialize an array with no element type", elementType);
				Guard.NotNull("Cannot serialize a null array", ArrayData);

				info.AddValue("ElementType", SerializationHelper.GetTypeNameForSerialization(elementType));
				info.AddValue("Rank", ArrayData.Rank);
				info.AddValue("TotalLength", ArrayData.Length);

				for (var dimension = 0; dimension < ArrayData.Rank; dimension++)
					info.AddValue("Length" + dimension, ArrayData.GetLength(dimension));

				for (var dimension = 0; dimension < ArrayData.Rank; dimension++)
					info.AddValue("LowerBound" + dimension, ArrayData.GetLowerBound(dimension));

				var i = 0;
				foreach (var obj in ArrayData)
				{
					info.AddValue("Item" + i, obj);
					i++;
				}
			}

			public void Deserialize(IXunitSerializationInfo info)
			{
				var arrType = SerializationHelper.GetType(info.GetValue<string>("ElementType"));
				if (arrType == null)
					throw new InvalidOperationException("Cannot deserialize array because array type is null");

				var rank = info.GetValue<int>("Rank");
				var totalLength = info.GetValue<int>("TotalLength");

				var lengths = new int[rank];
				var lowerBounds = new int[rank];
				for (var i = 0; i < lengths.Length; i++)
				{
					lengths[i] = info.GetValue<int>("Length" + i);
					lowerBounds[i] = info.GetValue<int>("LowerBound" + i);
				}

				ArrayData = Array.CreateInstance(arrType, lengths, lowerBounds);

				var indices = new int[rank];
				for (var i = 0; i < indices.Length; i++)
					indices[i] = lowerBounds[i];

				for (var i = 0; i < totalLength; i++)
				{
					var complete = false;

					for (var dim = rank - 1; dim >= 0; dim--)
					{
						if (indices[dim] >= lowerBounds[dim] + lengths[dim])
						{
							if (dim == 0)
							{
								complete = true;
								break;
							}

							for (var j = dim; j < rank; j++)
								indices[j] = lowerBounds[dim];

							indices[dim - 1]++;
						}
					}

					if (complete)
						break;

					var item = info.GetValue("Item" + i, arrType);
					ArrayData.SetValue(item, indices);
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
		public readonly object? Value;

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
		public XunitSerializationTriple(string key, object? value, Type type)
		{
			Key = key;
			Value = value;
			Type = type;
		}
	}
}
