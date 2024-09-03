using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Serializes and de-serializes objects. It supports a limited set of built-in types,
/// as well as anything which implements <see cref="IXunitSerializable"/>.
/// </summary>
public static class SerializationHelper
{
	static readonly char[] colonSeparator = [':'];
	static readonly Dictionary<TypeIndex, Func<string, object?>> deserializersByTypeIdx;
	static readonly Dictionary<Type, bool> enumSignsByType;
	static readonly Dictionary<TypeIndex, Func<object, Type, string>> serializersByTypeIdx;
	static readonly Dictionary<Type, TypeIndex> typeIndicesByType;
	static readonly Dictionary<TypeIndex, Type> typesByTypeIdx;

	static SerializationHelper()
	{
		static string extractValue(string v) =>
			v.Split(colonSeparator, 2).Last();

		static DateTimeStyles getDateStyle(string v) =>
			v.EndsWith("Z", StringComparison.Ordinal) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;

		var dateOnlyType = Type.GetType("System.DateOnly");
		var dateOnlyDayNumber = dateOnlyType?.GetProperty("DayNumber");
		var dateOnlyFromDayNumber = dateOnlyType?.GetMethod("FromDayNumber", BindingFlags.Public | BindingFlags.Static, null, [typeof(int)], null);

		var timeOnlyType = Type.GetType("System.TimeOnly");
		var timeOnlyTicks = timeOnlyType?.GetProperty("Ticks");
		var timeOnlyCtor = timeOnlyType?.GetConstructor([typeof(long)]);

		var indexType = Type.GetType("System.Index");
		var indexCtor = indexType?.GetConstructor([typeof(int), typeof(bool)]);

		var rangeType = Type.GetType("System.Range");
		var rangeCtor = indexType is null ? null : rangeType?.GetConstructor([indexType, indexType]);

		deserializersByTypeIdx = new()
		{
			{ TypeIndex.Type, SerializedTypeNameToType },
			{ TypeIndex.Enum, DeserializeEnum },
			{ TypeIndex.IXunitSerializable, DeserializeXunitSerializable },
			{ TypeIndex.TraitDictionary, DeserializeTraits },
			{ TypeIndex.Object, v => null },

			{ TypeIndex.String, FromBase64 },
			{ TypeIndex.Char, v => (char)ushort.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Byte, v => byte.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.SByte, v => sbyte.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Int16, v => short.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt16, v => ushort.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Int32, v => int.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt32, v => uint.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Int64, v => long.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt64, v => ulong.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Single, v => BitConverter.ToSingle((byte[])DeserializeArray(typeof(byte), extractValue(v)), 0) },
			{ TypeIndex.Double, v => BitConverter.ToDouble((byte[])DeserializeArray(typeof(byte), extractValue(v)), 0) },
			{ TypeIndex.Decimal, v => decimal.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Boolean, v => bool.Parse(v) },
			{ TypeIndex.DateTime, v => DateTime.Parse(v, CultureInfo.InvariantCulture, getDateStyle(v)) },
			{ TypeIndex.DateTimeOffset, v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture, getDateStyle(v)) },
			{ TypeIndex.TimeSpan, v => TimeSpan.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.BigInteger, v => BigInteger.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Version, Version.Parse },
		};

		if (dateOnlyFromDayNumber is not null)
			deserializersByTypeIdx.Add(TypeIndex.DateOnly, v => dateOnlyFromDayNumber.Invoke(null, [int.Parse(v, CultureInfo.InvariantCulture)]));
		if (timeOnlyCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.TimeOnly, v => timeOnlyCtor.Invoke([long.Parse(v, CultureInfo.InvariantCulture)]));
		if (indexCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.Index, v => DeserializeIndex(indexCtor, v));
		if (indexCtor is not null && rangeCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.Range, v => DeserializeRange(indexCtor, rangeCtor, v));

#pragma warning disable CA1065 // These thrown exceptions are done in lambdas, not directly in the static constructor

		serializersByTypeIdx = new()
		{
			{ TypeIndex.Type, (v, _) => TypeToSerializedTypeName((Type)v) },
			{ TypeIndex.Enum, SerializeEnum },
			{ TypeIndex.IXunitSerializable, (v, t) => SerializeXunitSerializable((IXunitSerializable)v, t) },
			{ TypeIndex.TraitDictionary, (v, _) => SerializeTraits((Dictionary<string, HashSet<string>>)v) },
			{ TypeIndex.Object, (_, __) => string.Empty },

			{ TypeIndex.String, (v, _) => ToBase64((string)v) },
			{ TypeIndex.Char, (v, _) => Convert.ToUInt16(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Byte, (v, _) => ((byte)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.SByte, (v, _) => ((sbyte)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Int16, (v, _) => ((short)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt16, (v, _) => ((ushort)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Int32, (v, _) => ((int)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt32, (v, _) => ((uint)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Int64, (v, _) => ((long)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.UInt64, (v, _) => ((ulong)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Single, (v, _) => "2[]:" + SerializeArray(BitConverter.GetBytes((float)v)) },
			{ TypeIndex.Double, (v, _) => "2[]:" + SerializeArray(BitConverter.GetBytes((double)v)) },
			{ TypeIndex.Decimal, (v, _) => ((decimal)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Boolean, (v, _) => v.ToString() ?? throw new InvalidOperationException("Boolean value returned null from ToString()") },
			{ TypeIndex.DateTime, (v, _) => ((DateTime)v).ToString("O", CultureInfo.InvariantCulture) },
			{ TypeIndex.DateTimeOffset, (v, _) => ((DateTimeOffset)v).ToString("O", CultureInfo.InvariantCulture) },
			{ TypeIndex.TimeSpan, (v, _) => ((TimeSpan)v).ToString("c", CultureInfo.InvariantCulture) },
			{ TypeIndex.BigInteger, (v, _) => ((BigInteger)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Version, (v, _) => ((Version)v).ToString() },
		};

		if (dateOnlyDayNumber is not null)
			serializersByTypeIdx.Add(TypeIndex.DateOnly, (v, _) => dateOnlyDayNumber.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call GetValue on an instance of '{0}': {1}", dateOnlyType!.SafeName(), v)));
		if (timeOnlyTicks is not null)
			serializersByTypeIdx.Add(TypeIndex.TimeOnly, (v, _) => timeOnlyTicks.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call Ticks on an instance of '{0}': {1}", timeOnlyType!.SafeName(), v)));
		if (indexCtor is not null)
			serializersByTypeIdx.Add(TypeIndex.Index, (v, _) => v.ToString() ?? throw new InvalidOperationException("Index value returned null from ToString()"));
		if (rangeCtor is not null)
			serializersByTypeIdx.Add(TypeIndex.Range, (v, _) => v.ToString() ?? throw new InvalidOperationException("Range value returned null from ToString()"));

#pragma warning restore CA1065

		typesByTypeIdx = new()
		{
			{ TypeIndex.Type, typeof(Type) },
			{ TypeIndex.Enum, typeof(Enum) },
			{ TypeIndex.IXunitSerializable, typeof(IXunitSerializable) },
			{ TypeIndex.TraitDictionary, typeof(Dictionary<string, HashSet<string>>) },
			{ TypeIndex.Object, typeof(object) },

			{ TypeIndex.String, typeof(string) },
			{ TypeIndex.Char, typeof(char) },
			{ TypeIndex.Byte, typeof(byte) },
			{ TypeIndex.SByte, typeof(sbyte) },
			{ TypeIndex.Int16, typeof(short) },
			{ TypeIndex.UInt16, typeof(ushort) },
			{ TypeIndex.Int32, typeof(int) },
			{ TypeIndex.UInt32, typeof(uint) },
			{ TypeIndex.Int64, typeof(long) },
			{ TypeIndex.UInt64, typeof(ulong) },
			{ TypeIndex.Single, typeof(float) },
			{ TypeIndex.Double, typeof(double) },
			{ TypeIndex.Decimal, typeof(decimal) },
			{ TypeIndex.Boolean, typeof(bool) },
			{ TypeIndex.DateTime, typeof(DateTime) },
			{ TypeIndex.DateTimeOffset, typeof(DateTimeOffset) },
			{ TypeIndex.TimeSpan, typeof(TimeSpan) },
			{ TypeIndex.BigInteger, typeof(BigInteger) },
			{ TypeIndex.Version, typeof(Version) },
		};

		if (dateOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.DateOnly, dateOnlyType);
		if (timeOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.TimeOnly, timeOnlyType);
		if (indexType is not null && indexCtor is not null)
			typesByTypeIdx.Add(TypeIndex.Index, indexType);
		if (rangeType is not null && rangeCtor is not null)
			typesByTypeIdx.Add(TypeIndex.Range, rangeType);

		typeIndicesByType = typesByTypeIdx.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

		enumSignsByType = new()
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
	}


	/// <summary>
	/// De-serializes an object.
	/// </summary>
	/// <typeparam name="T">The type of the object</typeparam>
	/// <param name="serializedValue">The object's serialized value</param>
	/// <returns>The de-serialized object</returns>
	public static T? Deserialize<T>(string serializedValue) =>
		(T?)Deserialize(serializedValue);

	/// <summary>
	/// De-serializes an object.
	/// </summary>
	/// <param name="serializedValue">The serialized value</param>
	/// <returns>The de-serialized object</returns>
	public static object? Deserialize(string serializedValue)
	{
		Guard.ArgumentNotNull(serializedValue);

		var pieces = serializedValue.Split(colonSeparator, 2);
		var typeIdxText = pieces[0];
		var isArray = false;
		var isNullable = false;

		if (typeIdxText.EndsWith("[]", StringComparison.Ordinal))
		{
			isArray = true;
			typeIdxText = typeIdxText.Substring(0, typeIdxText.Length - 2);
		}

		if (typeIdxText.EndsWith("?", StringComparison.Ordinal))
		{
			isNullable = true;
			typeIdxText = typeIdxText.Substring(0, typeIdxText.Length - 1);
		}

		if (!Enum.TryParse<TypeIndex>(typeIdxText, out var typeIdx) || typeIdx < TypeIndex_MinValue || typeIdx > TypeIndex_MaxValue)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Tried to deserialize unknown type index '{0}'", typeIdxText), nameof(serializedValue));

		if (!deserializersByTypeIdx.TryGetValue(typeIdx, out var deserializer))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot deserialize value of '{0}': unsupported platform", typeIdx), nameof(serializedValue));

		if (pieces.Length != 2)
			return null;

		if (isArray)
		{
			var elementType = typesByTypeIdx[typeIdx];
			if (isNullable)
				elementType = typeof(Nullable<>).MakeGenericType(elementType);

			return DeserializeArray(elementType, pieces[1]);
		}

		return deserializer(pieces[1]);
	}

	static Array DeserializeArray(
		Type elementType,
		string serializedArray)
	{
		var serializer = new ArraySerializer(elementType);
		var info = new XunitSerializationInfo(serializedArray);
		serializer.Deserialize(info);
		return serializer.ArrayData;
	}

	static T? DeserializeEmbeddedTypeValue<T>(
		string serializedValue,
		Func<Type, string, T?> converter)
	{
		var pieces = serializedValue.Split(colonSeparator, 2);
		if (pieces.Length == 1)
			return default;

		var type =
			SerializedTypeNameToType(FromBase64(pieces[0]))
				?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Serialized type name '{0}' could not be converted into a Type object.", pieces[0]));

		return converter(type, pieces[1]);
	}

	static object? DeserializeEnum(string serializedValue) =>
		DeserializeEmbeddedTypeValue(
			serializedValue,
			(type, embeddedValue) =>
				type.IsEnum
					? Enum.Parse(type, embeddedValue)
					: throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize type '{0}' which was not an enum", type.SafeName()))
		);

	static object? DeserializeIndex(
		ConstructorInfo indexCtor,
		string serializedValue)
	{
		try
		{
			return
				serializedValue.StartsWith("^", StringComparison.Ordinal)
					? indexCtor.Invoke([int.Parse(serializedValue.Substring(1), CultureInfo.InvariantCulture), true])
					: indexCtor.Invoke([int.Parse(serializedValue, CultureInfo.InvariantCulture), false]);
		}
		catch
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize invalid System.Index value '{0}'", serializedValue));
		}
	}

	static object? DeserializeRange(
		ConstructorInfo indexCtor,
		ConstructorInfo rangeCtor,
		string serializedValue)
	{
		try
		{
			var idxSeparator = serializedValue.IndexOf("..", StringComparison.InvariantCulture);
			if (idxSeparator > 0)
				return rangeCtor.Invoke([
					DeserializeIndex(indexCtor, serializedValue.Substring(0, idxSeparator)),
					DeserializeIndex(indexCtor, serializedValue.Substring(idxSeparator + 2))
				]);
		}
		catch { }

		throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize invalid System.Range value '{0}'", serializedValue));
	}

	static Dictionary<string, HashSet<string>> DeserializeTraits(string serializedValue)
	{
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		if (serializedValue.Length != 0)
		{
			var pieces = FromBase64(serializedValue).Split('\n');
			var keys = FromBase64(pieces[0]).Split('\n');

			// Safeguard for missing data, which should never happen
			if (pieces.Length == keys.Length + 1)
			{
				var idx = 1;

				foreach (var key in keys)
				{
					var list = new HashSet<string>();
					var valuePieces = FromBase64(pieces[idx++]).Split('\n');

					foreach (var valuePiece in valuePieces)
						list.Add(FromBase64(valuePiece));

					result[FromBase64(key)] = list;
				}
			}
		}

		return result;
	}

	static IXunitSerializable? DeserializeXunitSerializable(string serializedValue) =>
		DeserializeEmbeddedTypeValue(serializedValue, (type, embeddedValue) =>
		{
			if (!typeof(IXunitSerializable).IsAssignableFrom(type))
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Attempted to deserialize type '{0}' which did not implement {1}.",
						type.SafeName(),
						typeof(IXunitSerializable).SafeName()
					)
				);

			var serializationInfo = new XunitSerializationInfo(embeddedValue);

			try
			{
				if (Activator.CreateInstance(type) is not IXunitSerializable value)
					throw new InvalidOperationException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Attempted to deserialize type '{0}' which did not implement {1}.",
							type.SafeName(),
							typeof(IXunitSerializable).SafeName()
						)
					);

				value.Deserialize(serializationInfo);
				return value;
			}
			catch (MissingMemberException)
			{
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Could not de-serialize type '{0}' because it lacks a parameterless constructor.",
						type.SafeName()
					)
				);
			}
		});

	internal static string FromBase64(string serializedValue)
	{
		var bytes = Convert.FromBase64String(serializedValue);
		return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	/// <summary>
	/// Determines if an object instance is serializable.
	/// </summary>
	/// <param name="value">The object to test for serializability.</param>
	/// <returns>Returns <c>true</c> if the object can be serialized; <c>false</c>, otherwise.</returns>
	/// <remarks>
	/// As <c>null</c> values always return <c>true</c>, even if the underlying type (which is unknown)
	/// might not be serializable, it's better to test via <see cref="IsSerializable(object?, Type?)"/>
	/// whenever possible.
	/// </remarks>
	public static bool IsSerializable(object? value) =>
		value is null || IsSerializable(value, value.GetType());

	/// <summary>
	/// Determines if a given type supports serialization.
	/// </summary>
	/// <param name="value">The object to test for serializability.</param>
	/// <param name="type">The type to test for serializability.</param>
	/// <returns>Returns <c>true</c> if objects of the given type can be serialized; <c>false</c>, otherwise.</returns>
	public static bool IsSerializable(
		object? value,
		Type? type)
	{
		if (type is null || type == typeof(object))
			return value is null;

		// We can only serialize fully realized types (with full names); this excludes things like
		// generic type arguments, generic array type arguments, generic pointer types, or generic byref types.
		if (value is Type typeValue)
			return typeValue.FullName is not null;

		// You usually get instances of RuntimeType, not the abstract Type
		if (typeof(Type).IsAssignableFrom(type))
			return true;

		if (type.IsArray)
		{
			// Start by making sure we're comfortable with the array type itself
			if (!IsSerializable(null, type.GetElementType()))
				return false;

			// Then if we can, we want to verify every value in the array is okay
			if (value is Array valueArray)
				return valueArray.Cast<object?>().All(item => IsSerializable(item, item?.GetType()));
		}

		if (type.IsEnum || type.IsNullableEnum())
			return type.IsFromLocalAssembly();

		if (type.Implements(typeof(IXunitSerializable)))
			return true;

		type = type.UnwrapNullable();
		return typeIndicesByType.ContainsKey(type);
	}

	/// <summary>
	/// Serializes an object.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <param name="type">The type of the value to be serialized (cannot be <c>null</c> if <paramref name="value"/> is <c>null</c>)</param>
	/// <returns>The serialized object</returns>
	public static string Serialize(
		object? value,
		Type? type = null)
	{
		type ??= value?.GetType() ?? typeof(object);

		if (value is null && !type.IsNullable())
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a null value as type '{0}' because it's type-incompatible", type.SafeName()), nameof(value));
		if (value is not null && !type.IsAssignableFrom(value.GetType()))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}' as type '{1}' because it's type-incompatible", value.GetType().SafeName(), type.SafeName()), nameof(value));

		var coreValueType = type;
		var isArray = type.IsArray;
		if (isArray)
			coreValueType = type.GetElementType()!;  // We know GetElementType() will not return null for arrays

		var nonNullableCoreValueType = coreValueType.UnwrapNullable();

		TypeIndex typeIdx;
		if (nonNullableCoreValueType.IsEnum)
			typeIdx = TypeIndex.Enum;
		else if (nonNullableCoreValueType.Implements(typeof(IXunitSerializable)))
			typeIdx = TypeIndex.IXunitSerializable;
		else if (typeof(Type).IsAssignableFrom(nonNullableCoreValueType))
			typeIdx = TypeIndex.Type;
		else
		{
			var kvp = typeIndicesByType.FirstOrDefault(kvp => nonNullableCoreValueType == kvp.Key);
			if (kvp.Key is null)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported type for serialization", type.SafeName()), nameof(value));
			typeIdx = kvp.Value;
		}

		if (!serializersByTypeIdx.TryGetValue(typeIdx, out var serializer))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported platform", typeIdx), nameof(value));

		var typeIdxText = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", (int)typeIdx, coreValueType != nonNullableCoreValueType ? "?" : "", isArray ? "[]" : "");

		return
			value is null
				? typeIdxText
				: isArray
					? string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, SerializeArray((Array)value))
					: typeIdx != TypeIndex.Object
						? string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, serializer(value, nonNullableCoreValueType))
						: throw new ArgumentException("Cannot serialize a non-null value of type 'System.Object'", nameof(value));
	}

	static string SerializeArray(Array array)
	{
		var info = new XunitSerializationInfo();
		var arraySer = new ArraySerializer(array);
		arraySer.Serialize(info);
		return info.ToSerializedString();
	}

	static string SerializeEmbeddedTypeValue(
		string? value,
		Type type) =>
			string.Format(CultureInfo.InvariantCulture, "{0}:{1}", ToBase64(TypeToSerializedTypeName(type)), value);

	static string SerializeEnum(
		object value,
		Type type)
	{
		if (!type.IsFromLocalAssembly())
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because it lives in the GAC", type.SafeName(), value), nameof(value));

		Type underlyingType;

		try
		{
			underlyingType = Enum.GetUnderlyingType(value.GetType());
		}
		catch (Exception ex)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because an exception was thrown getting its underlying type", type.SafeName(), value), ex);
		}

		if (!enumSignsByType.TryGetValue(underlyingType, out var signed))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because the underlying type '{2}' is not supported", type.SafeName(), value, underlyingType.SafeName()), nameof(value));

		var result =
			signed
				? Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
				: Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

		return SerializeEmbeddedTypeValue(result, type);
	}

	static string SerializeTraits(Dictionary<string, HashSet<string>>? value)
	{
		if (value is null || value.Count == 0)
			return string.Empty;

		var result = new StringBuilder();

		var keysArray = value.Keys.ToArray();
		result.Append(ToBase64(string.Join("\n", keysArray.Select(ToBase64))));

		foreach (var key in keysArray)
		{
			result.Append('\n');
			result.Append(ToBase64(string.Join("\n", value[key].Select(ToBase64))));
		}

		return ToBase64(result.ToString());
	}

	static string SerializeXunitSerializable(
		IXunitSerializable value,
		Type type)
	{
		var info = new XunitSerializationInfo();
		value.Serialize(info);

		return SerializeEmbeddedTypeValue(info.ToSerializedString(), type);
	}

	/// <summary>
	/// Converts a type name (in "TypeName" format for mscorlib types, or "TypeName,AssemblyName" format for
	/// all others) into a <see cref="Type"/> object.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name ("TypeName,AssemblyName")</param>
	/// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
	public static Type? SerializedTypeNameToType(string assemblyQualifiedTypeName) =>
		TypeHelper.GetType(assemblyQualifiedTypeName);

	/// <summary>
	/// Gets an assembly qualified type name for serialization.
	/// </summary>
	/// <param name="value">The type to get the name for</param>
	/// <returns>A string in "TypeName" format (for mscorlib types) or "TypeName,AssemblyName" format (for all others)</returns>
	public static string TypeToSerializedTypeName(Type value) =>
		TypeHelper.GetTypeName(value);

	internal static string ToBase64(string value) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

	sealed class ArraySerializer : IXunitSerializable
	{
		Array? array;
		readonly Type? elementType;

		public Array ArrayData => Guard.NotNull("Array must not be null (did you forget to deserialize?)", array);

		public ArraySerializer(Array array)
		{
			Guard.ArgumentNotNull(array);

			elementType = array.GetType().GetElementType();
			Guard.ArgumentNotNull("The element type of the array is unknown", elementType, nameof(array));

			this.array = array;
		}

		public ArraySerializer(Type elementType)
		{
			Guard.ArgumentNotNull(elementType);

			this.elementType = elementType;
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			Guard.NotNull("Element type must not be null", elementType);

			var rank = info.GetValue<int>("r");
			var totalLength = info.GetValue<int>("tl");

			var lengths = new int[rank];
			var lowerBounds = new int[rank];
			for (var i = 0; i < lengths.Length; i++)
			{
				lengths[i] = info.GetValue<int>(string.Format(CultureInfo.InvariantCulture, "l{0}", i));
				lowerBounds[i] = info.GetValue<int>(string.Format(CultureInfo.InvariantCulture, "lb{0}", i));
			}

			array = Array.CreateInstance(elementType, lengths, lowerBounds);

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

				var item = info.GetValue(string.Format(CultureInfo.InvariantCulture, "i{0}", i));
				array.SetValue(item, indices);
				indices[rank - 1]++;
			}
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			Guard.NotNull("Array must not be null", array);

			info.AddValue("r", array.Rank);
			info.AddValue("tl", array.Length);

			for (var dimension = 0; dimension < array.Rank; dimension++)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "l{0}", dimension), array.GetLength(dimension));
			for (var dimension = 0; dimension < array.Rank; dimension++)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "lb{0}", dimension), array.GetLowerBound(dimension));

			var i = 0;
			foreach (var obj in array)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "i{0}", i++), obj, obj?.GetType() ?? elementType);
		}
	}

	enum TypeIndex
	{
		// Special cases
		Type = -5,                // Custom serialization of the type name
		IXunitSerializable = -4,  // Supports any object which implements IXunitSerializable
		Enum = -3,                // Supports any (non-GAC'd) enum value
		TraitDictionary = -2,     // Only supports Dictionary<string, HashSet<string>> for traits
		Object = -1,              // Only arrays and null values

		// Supported built-in types
		String = 0,
		Char = 1,
		Byte = 2,
		SByte = 3,
		Int16 = 4,
		UInt16 = 5,
		Int32 = 6,
		UInt32 = 7,
		Int64 = 8,
		UInt64 = 9,
		Single = 10,
		Double = 11,
		Decimal = 12,
		Boolean = 13,
		DateTime = 14,
		DateTimeOffset = 15,
		TimeSpan = 16,
		BigInteger = 17,
		DateOnly = 18,
		TimeOnly = 19,
		Version = 20,
		Index = 21,
		Range = 22,
	}

	const TypeIndex TypeIndex_MinValue = TypeIndex.Type;
	const TypeIndex TypeIndex_MaxValue = TypeIndex.Range;
}
