using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Serializes and de-serializes objects. It supports a limited set of built-in types,
/// as well as anything which implements <see cref="IXunitSerializable"/>.
/// </summary>
public static class SerializationHelper
{
	internal static readonly _ITypeInfo TypeInfo_IXunitSerializable = Reflector.Wrap(typeof(IXunitSerializable));
	internal static readonly _ITypeInfo TypeInfo_Object = Reflector.Wrap(typeof(object));
	internal static readonly _ITypeInfo TypeInfo_Type = Reflector.Wrap(typeof(Type));

	static readonly char[] colonSeparator = new[] { ':' };
	static readonly Dictionary<TypeIndex, Func<string, object?>> deserializersByTypeIdx;
	static readonly Dictionary<Type, bool> enumSignsByType;
	static readonly Dictionary<TypeIndex, Func<object, _ITypeInfo, string>> serializersByTypeIdx;
	static readonly Dictionary<Type, TypeIndex> typeIndicesByType;
	static readonly Dictionary<TypeIndex, Type> typesByTypeIdx;

	static SerializationHelper()
	{
		static string extractValue(string v) =>
			v.Split(new[] { ':' }, 2).Last();

		static DateTimeStyles getDateStyle(string v) =>
			v.EndsWith("Z", StringComparison.Ordinal) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;

		var dateOnlyType = Type.GetType("System.DateOnly");
		var dateOnlyDayNumber = dateOnlyType?.GetProperty("DayNumber");
		var dateOnlyFromDayNumber = dateOnlyType?.GetMethod("FromDayNumber", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);

		var timeOnlyType = Type.GetType("System.TimeOnly");
		var timeOnlyTicks = timeOnlyType?.GetProperty("Ticks");
		var timeOnlyCtor = timeOnlyType?.GetConstructor(new[] { typeof(long) });

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
		};

		if (dateOnlyFromDayNumber is not null)
			deserializersByTypeIdx.Add(TypeIndex.DateOnly, v => dateOnlyFromDayNumber.Invoke(null, new object[] { int.Parse(v, CultureInfo.InvariantCulture) }));
		if (timeOnlyCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.TimeOnly, v => timeOnlyCtor.Invoke(new object[] { long.Parse(v, CultureInfo.InvariantCulture) }));

#pragma warning disable CA1065 // These thrown exceptions are done in lambdas, not directly in the static constructor

		serializersByTypeIdx = new()
		{
			{ TypeIndex.Type, (v, _) => TypeToSerializedTypeName((Type)v) },
			{ TypeIndex.Enum, SerializeEnum },
			{ TypeIndex.IXunitSerializable, (v, t) => SerializeXunitSerializable((IXunitSerializable)v, t) },
			{ TypeIndex.TraitDictionary, (v, _) => SerializeTraits((Dictionary<string, List<string>>)v) },
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
		};

		if (dateOnlyDayNumber is not null)
			serializersByTypeIdx.Add(TypeIndex.DateOnly, (v, _) => dateOnlyDayNumber.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call GetValue on an instance of '{0}': {1}", dateOnlyType!.SafeName(), v)));
		if (timeOnlyTicks is not null)
			serializersByTypeIdx.Add(TypeIndex.TimeOnly, (v, _) => timeOnlyTicks.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call Ticks on an instance of '{0}': {1}", timeOnlyType!.SafeName(), v)));

#pragma warning restore CA1065

		typesByTypeIdx = new()
		{
			{ TypeIndex.Type, typeof(Type) },
			{ TypeIndex.Enum, typeof(Enum) },
			{ TypeIndex.IXunitSerializable, typeof(IXunitSerializable) },
			{ TypeIndex.TraitDictionary, typeof(Dictionary<string, List<string>>) },
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
		};

		if (dateOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.DateOnly, dateOnlyType);
		if (timeOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.TimeOnly, timeOnlyType);

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

		var type = SerializedTypeNameToType(FromBase64(pieces[0]));
		if (type is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Serialized type name '{0}' could not be converted into a Type object.", pieces[0]));

		return converter(type, pieces[1]);
	}

	static object? DeserializeEnum(string serializedValue) =>
		DeserializeEmbeddedTypeValue(serializedValue, (type, embeddedValue) =>
		{
			if (!type.IsEnum)
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize type '{0}' which was not an enum", type.SafeName()));

			return Enum.Parse(type, embeddedValue);
		});

	static Dictionary<string, List<string>> DeserializeTraits(string serializedValue)
	{
		var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

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
					var list = new List<string>();
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
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize type '{0}' which did not implement {1}.", type.SafeName(), typeof(IXunitSerializable).FullName));

			var serializationInfo = new XunitSerializationInfo(embeddedValue);

			try
			{
				if (Activator.CreateInstance(type) is not IXunitSerializable value)
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Attempted to deserialize type '{0}' which did not implement {1}.", type.SafeName(), typeof(IXunitSerializable).FullName));

				value.Deserialize(serializationInfo);
				return value;
			}
			catch (MissingMemberException)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not de-serialize type '{0}' because it lacks a parameterless constructor.", type.SafeName()));
			}
		});

	internal static string FromBase64(string serializedValue)
	{
		var bytes = Convert.FromBase64String(serializedValue);
		return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	/// <summary>
	/// Determines if an object instance is serializable. Note that null values always return true,
	/// even if the underlying type (which is unknown) might not be serializable, so it's better to
	/// test via <see cref="IsSerializable(object, Type)"/> or <see cref="IsSerializable(object, _ITypeInfo)"/>
	/// whenever possible.
	/// </summary>
	/// <param name="value">The object to test for serializability.</param>
	/// <returns>Returns <c>true</c> if the object can be serialized; <c>false</c>, otherwise.</returns>
	public static bool IsSerializable(object? value)
	{
		if (value is null)
			return true;

		return IsSerializable(value, value.GetType());
	}

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

		// You usually get instances of RuntimeType, not the abstract Type
		if (typeof(Type).IsAssignableFrom(type))
			return true;

		if (type.IsArray)
			return IsSerializable(null, type.GetElementType());

		if (type.IsEnum || type.IsNullableEnum())
			return type.IsFromLocalAssembly();

		if (type.Implements(typeof(IXunitSerializable)))
			return true;

		type = type.UnwrapNullable();
		return typeIndicesByType.ContainsKey(type);
	}

	/// <summary>
	/// Determines if a given type supports serialization.
	/// </summary>
	/// <param name="value">The object to test for serializability.</param>
	/// <param name="typeInfo">The type to test for serializability.</param>
	/// <returns>Returns <c>true</c> if objects of the given type can be serialized; <c>false</c>, otherwise.</returns>
	public static bool IsSerializable(
		object? value,
		_ITypeInfo? typeInfo)
	{
		if (typeInfo is null || typeInfo.Equal(typeof(object)))
			return value is null;

		// You usually get instances of RuntimeType, not the abstract Type
		if (TypeInfo_Type.IsAssignableFrom(typeInfo))
			return true;

		if (typeInfo.IsArray)
			return IsSerializable(null, typeInfo.GetElementType());

		if (typeInfo.IsEnum || typeInfo.IsNullableEnum())
			return typeInfo.IsFromLocalAssembly();

		if (typeInfo.Implements(TypeInfo_IXunitSerializable))
			return true;

		typeInfo = typeInfo.UnwrapNullable();
		if (typeIndicesByType.Keys.Any(st => typeInfo.Equal(st)))
			return true;

		return false;
	}

	/// <summary>
	/// Serializes an object.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <param name="type">The type of the value to be serialized (cannot be <c>null</c> if <paramref name="value"/> is <c>null</c>)</param>
	/// <returns>The serialized object</returns>
	public static string Serialize(
		object? value,
		Type? type) =>
			Serialize(value, Reflector.Wrap(type));

	/// <summary>
	/// Serializes an object.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <param name="typeInfo">The type of the value to be serialized (cannot be <c>null</c> if <paramref name="value"/> is <c>null</c>)</param>
	/// <returns>The serialized object</returns>
	public static string Serialize(
		object? value,
		_ITypeInfo? typeInfo = null)
	{
		typeInfo ??= Reflector.Wrap(value?.GetType()) ?? TypeInfo_Object;

		if (value is null && !typeInfo.IsNullable())
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a null value as type '{0}' because it's type-incompatible", typeInfo.Name), nameof(value));
		if (value is not null && !typeInfo.IsAssignableFrom(value.GetType()))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}' as type '{1}' because it's type-incompatible", value.GetType().SafeName(), typeInfo.Name), nameof(value));

		var coreValueTypeInfo = typeInfo;
		var isArray = typeInfo.IsArray;
		if (isArray)
			coreValueTypeInfo = typeInfo.GetElementType()!;  // We know GetElementType() will not return null for arrays

		var nonNullableCoreValueTypeInfo = coreValueTypeInfo.UnwrapNullable();

		TypeIndex typeIdx;
		if (nonNullableCoreValueTypeInfo.IsEnum)
			typeIdx = TypeIndex.Enum;
		else if (nonNullableCoreValueTypeInfo.Implements(TypeInfo_IXunitSerializable))
			typeIdx = TypeIndex.IXunitSerializable;
		else if (TypeInfo_Type.IsAssignableFrom(nonNullableCoreValueTypeInfo))
			typeIdx = TypeIndex.Type;
		else
		{
			var kvp = typeIndicesByType.FirstOrDefault(kvp => nonNullableCoreValueTypeInfo.Equal(kvp.Key));
			if (kvp.Key is null)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported type for serialization", typeInfo.Name), nameof(value));
			typeIdx = kvp.Value;
		}

		if (!serializersByTypeIdx.TryGetValue(typeIdx, out var serializer))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported platform", typeIdx), nameof(value));

		var typeIdxText = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", (int)typeIdx, coreValueTypeInfo != nonNullableCoreValueTypeInfo ? "?" : "", isArray ? "[]" : "");

		if (value is null)
			return typeIdxText;

		if (isArray)
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, SerializeArray((Array)value));

		if (typeIdx == TypeIndex.Object)
			throw new ArgumentException("Cannot serialize a non-null value of type 'System.Object'", nameof(value));

		return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, serializer(value, nonNullableCoreValueTypeInfo));
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
		_ITypeInfo typeInfo) =>
			string.Format(CultureInfo.InvariantCulture, "{0}:{1}", ToBase64(TypeToSerializedTypeName(typeInfo)), value);

	static string SerializeEnum(
		object value,
		_ITypeInfo typeInfo)
	{
		if (!typeInfo.IsFromLocalAssembly())
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because it lives in the GAC", typeInfo.Name, value), nameof(value));

		Type underlyingType;

		try
		{
			underlyingType = Enum.GetUnderlyingType(value.GetType());
		}
		catch (Exception ex)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because an exception was thrown getting its underlying type", typeInfo.Name, value), ex);
		}

		if (!enumSignsByType.TryGetValue(underlyingType, out var signed))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize enum '{0}.{1}' because the underlying type '{2}' is not supported", typeInfo.Name, value, underlyingType.SafeName()), nameof(value));

		var result =
			signed
				? Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
				: Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

		return SerializeEmbeddedTypeValue(result, typeInfo);
	}

	static string SerializeTraits(Dictionary<string, List<string>>? value)
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
		_ITypeInfo typeInfo)
	{
		var info = new XunitSerializationInfo();
		value.Serialize(info);

		return SerializeEmbeddedTypeValue(info.ToSerializedString(), typeInfo);
	}

	/// <summary>
	/// Converts a type name (in "TypeName" format for mscorlib types, or "TypeName,AssemblyName" format for
	/// all others) into a <see cref="Type"/> object.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name ("TypeName,AssemblyName")</param>
	/// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
	public static Type? SerializedTypeNameToType(string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		var firstOpenSquare = assemblyQualifiedTypeName.IndexOf('[');
		if (firstOpenSquare > 0)
		{
			var backtick = assemblyQualifiedTypeName.IndexOf('`');
			if (backtick > 0 && backtick < firstOpenSquare)
			{
				// Run the string looking for the matching closing square brace. Can't just assume the last one
				// is the end, since the type could be trailed by array designators.
				var depth = 1;
				var lastOpenSquare = firstOpenSquare + 1;
				var sawNonArrayDesignator = false;
				for (; depth > 0 && lastOpenSquare < assemblyQualifiedTypeName.Length; ++lastOpenSquare)
				{
					switch (assemblyQualifiedTypeName[lastOpenSquare])
					{
						case '[': ++depth; break;
						case ']': --depth; break;
						case ',': break;
						default: sawNonArrayDesignator = true; break;
					}
				}

				if (sawNonArrayDesignator)
				{
					if (depth != 0)  // Malformed, because we never closed what we opened
						return null;

					var genericArgument = assemblyQualifiedTypeName.Substring(firstOpenSquare + 1, lastOpenSquare - firstOpenSquare - 2);  // Strip surrounding [ and ]
					var innerTypeNames = SplitAtOuterCommas(genericArgument).Select(x => x.Substring(1, x.Length - 2)).ToArray();  // Strip surrounding [ and ] from each type name
					var innerTypes = innerTypeNames.Select(s => SerializedTypeNameToType(s)).WhereNotNull().ToArray();

					// Lengths won't be equal if any types failed to load (they'd show up as null, and we filtered out nulls)
					if (innerTypes.Length != innerTypeNames.Length)
						return null;

					var genericDefinitionName = assemblyQualifiedTypeName.Substring(0, firstOpenSquare) + assemblyQualifiedTypeName.Substring(lastOpenSquare);
					var genericDefinition = SerializedTypeNameToType(genericDefinitionName);
					if (genericDefinition is null)
						return null;

					// Push array ranks so we can get down to the actual generic definition
					var arrayRanks = new Stack<int>();
					while (true)
					{
						var elementType = genericDefinition.GetElementType();
						if (elementType is null)
							break;

						arrayRanks.Push(genericDefinition.GetArrayRank());
						genericDefinition = elementType;
					}

					var closedGenericType = genericDefinition.MakeGenericType(innerTypes);
					while (arrayRanks.Count > 0)
					{
						var rank = arrayRanks.Pop();
						closedGenericType = rank > 1 ? closedGenericType.MakeArrayType(rank) : closedGenericType.MakeArrayType();
					}

					return closedGenericType;
				}
			}
		}

		var parts = SplitAtOuterCommas(assemblyQualifiedTypeName, true);
		if (parts.Count == 0)
			return null;
		if (parts.Count == 1)
			return Type.GetType(parts[0]);

		var typeName = parts[0];
		var assemblyName = parts[1];
		var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
		if (assembly is null)
		{
			try
			{
				assembly = Assembly.Load(assemblyName);
			}
			catch { }
		}

		if (assembly is null)
			return null;

		return assembly.GetType(typeName);
	}

	static IList<string> SplitAtOuterCommas(string value, bool trimWhitespace = false)
	{
		var results = new List<string>();

		var startIndex = 0;
		var endIndex = 0;
		var depth = 0;

		for (; endIndex < value.Length; ++endIndex)
		{
			switch (value[endIndex])
			{
				case '[': ++depth; break;
				case ']': --depth; break;
				case ',':
					if (depth == 0)
					{
						results.Add(trimWhitespace ?
							SubstringTrim(value, startIndex, endIndex - startIndex) :
							value.Substring(startIndex, endIndex - startIndex));
						startIndex = endIndex + 1;
					}
					break;
			}
		}

		if (depth != 0 || startIndex >= endIndex)
		{
			results.Clear();
		}
		else
		{
			results.Add(trimWhitespace ?
				SubstringTrim(value, startIndex, endIndex - startIndex) :
				value.Substring(startIndex, endIndex - startIndex));
		}

		return results;
	}

	static string SubstringTrim(string str, int startIndex, int length)
	{
		int endIndex = startIndex + length;

		while (startIndex < endIndex && char.IsWhiteSpace(str[startIndex]))
			startIndex++;

		while (endIndex > startIndex && char.IsWhiteSpace(str[endIndex - 1]))
			endIndex--;

		return str.Substring(startIndex, endIndex - startIndex);
	}

	/// <summary>
	/// Gets an assembly qualified type name for serialization.
	/// </summary>
	/// <param name="type">The type to get the name for</param>
	/// <returns>A string in "TypeName" format (for mscorlib types) or "TypeName,AssemblyName" format (for all others)</returns>
	public static string TypeToSerializedTypeName(Type type) =>
		TypeToSerializedTypeName(Reflector.Wrap(type));

	/// <summary>
	/// Gets an assembly qualified type name for serialization.
	/// </summary>
	/// <param name="typeInfo">The type to get the name for</param>
	/// <returns>A string in "TypeName" format (for mscorlib types) or "TypeName,AssemblyName" format (for all others)</returns>
	public static string TypeToSerializedTypeName(_ITypeInfo typeInfo)
	{
		// Use the abstract Type instead of concretes like RuntimeType
		if (typeof(Type).IsAssignableFrom(typeInfo))
			typeInfo = TypeInfo_Type;

		if (!typeInfo.IsFromLocalAssembly())
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize type '{0}' because it lives in the GAC", typeInfo.Name), nameof(typeInfo));

		var typeToMap = typeInfo;
		var typeName = typeToMap.Name;
		var assemblyName = typeToMap.Assembly.Name.Split(',')[0];

		var arrayRanks = new Stack<int>();
		while (true)
		{
			var elementType = typeToMap.GetElementType();
			if (elementType is null)
				break;

			arrayRanks.Push(typeToMap.GetArrayRank());
			typeToMap = elementType;
		}

		if (typeToMap.IsGenericType && !typeToMap.IsGenericTypeDefinition)
		{
			var typeDefinition = typeToMap.GetGenericTypeDefinition();
			var innerTypes =
				typeToMap
					.GetGenericArguments()
					.Select(t => string.Format(CultureInfo.InvariantCulture, "[{0}]", TypeToSerializedTypeName(t)))
					.ToArray();

			typeName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", typeDefinition.Name, string.Join(",", innerTypes));

			while (arrayRanks.Count > 0)
			{
				typeName += '[';
				for (var commas = arrayRanks.Pop() - 1; commas > 0; --commas)
					typeName += ',';
				typeName += ']';
			}
		}

		if (string.Equals(assemblyName, "mscorlib", StringComparison.OrdinalIgnoreCase) ||
			string.Equals(assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase))
			return typeName;

		return string.Format(CultureInfo.InvariantCulture, "{0},{1}", typeName, assemblyName);
	}

	internal static string ToBase64(string value)
	{
		var bytes = Encoding.UTF8.GetBytes(value);
		return Convert.ToBase64String(bytes);
	}

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

			int[] indices = new int[rank];
			for (int i = 0; i < indices.Length; i++)
				indices[i] = lowerBounds[i];

			for (int i = 0; i < totalLength; i++)
			{
				var complete = false;

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

			for (int dimension = 0; dimension < array.Rank; dimension++)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "l{0}", dimension), array.GetLength(dimension));
			for (int dimension = 0; dimension < array.Rank; dimension++)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "lb{0}", dimension), array.GetLowerBound(dimension));

			int i = 0;
			foreach (object obj in array)
				info.AddValue(string.Format(CultureInfo.InvariantCulture, "i{0}", i++), obj, obj?.GetType() ?? elementType);
		}
	}

	enum TypeIndex
	{
		// Special cases
		Type = -5,                // Custom serialization of the type name
		IXunitSerializable = -4,  // Supports any object which implements IXunitSerializable
		Enum = -3,                // Supports any (non-GAC'd) enum value
		TraitDictionary = -2,     // Only supports Dictionary<string, List<string>> for traits
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
	}

	const TypeIndex TypeIndex_MinValue = TypeIndex.Type;
	const TypeIndex TypeIndex_MaxValue = TypeIndex.TimeOnly;
}
