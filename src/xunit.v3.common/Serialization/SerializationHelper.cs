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
/// as well as anything which implements <see cref="IXunitSerializable"/>. Custom serializers
/// can implement <see cref="IXunitSerializer"/> and register by decorating the test
/// assembly with <see cref="RegisterXunitSerializerAttribute"/>.
/// </summary>
public class SerializationHelper
{
	static readonly char[] colonSeparator = [':'];
	static readonly Type? dateOnlyType = Type.GetType("System.DateOnly");
	static readonly PropertyInfo? dateOnlyDayNumber = dateOnlyType?.GetProperty("DayNumber");
	static readonly MethodInfo? dateOnlyFromDayNumber = dateOnlyType?.GetMethod("FromDayNumber", BindingFlags.Public | BindingFlags.Static, null, [typeof(int)], null);
	static readonly Type? indexType = Type.GetType("System.Index");
	static readonly ConstructorInfo? indexCtor = indexType?.GetConstructor([typeof(int), typeof(bool)]);
	static readonly Type? rangeType = Type.GetType("System.Range");
	static readonly ConstructorInfo? rangeCtor = indexType is null ? null : rangeType?.GetConstructor([indexType, indexType]);
	static readonly Type? timeOnlyType = Type.GetType("System.TimeOnly");
	static readonly PropertyInfo? timeOnlyTicks = timeOnlyType?.GetProperty("Ticks");
	static readonly ConstructorInfo? timeOnlyCtor = timeOnlyType?.GetConstructor([typeof(long)]);
	static readonly Dictionary<Type, TypeIndex> typeIndicesByType;
	static readonly Dictionary<TypeIndex, Type> typesByTypeIdx;

	readonly Dictionary<TypeIndex, Func<string, object?>> deserializersByTypeIdx;
	readonly Dictionary<TypeIndex, Func<object, Type, string>> serializersByTypeIdx;
	readonly Dictionary<Type, IXunitSerializer> xunitSeralizersByType;

	static SerializationHelper()
	{
		typesByTypeIdx = new()
		{
			{ TypeIndex.Type, typeof(Type) },
			{ TypeIndex.IXunitSerializer, typeof(object) },
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
			{ TypeIndex.Guid, typeof(Guid) },
			{ TypeIndex.Uri, typeof(Uri) },
		};

		if (dateOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.DateOnly, dateOnlyType);
		if (timeOnlyType is not null)
			typesByTypeIdx.Add(TypeIndex.TimeOnly, timeOnlyType);
		if (indexType is not null && indexCtor is not null)
			typesByTypeIdx.Add(TypeIndex.Index, indexType);
		if (rangeType is not null && rangeCtor is not null)
			typesByTypeIdx.Add(TypeIndex.Range, rangeType);

		// Exclude IXunitSerializer since it duplicates typeof(object)
		typeIndicesByType =
			typesByTypeIdx
				.Where(kvp => kvp.Key != TypeIndex.IXunitSerializer)
				.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="SerializationHelper"/> class.
	/// </summary>
	protected SerializationHelper()
	{
		static DateTimeStyles getDateStyle(string v) =>
			v.EndsWith("Z", StringComparison.Ordinal) ? DateTimeStyles.AdjustToUniversal : DateTimeStyles.None;

		deserializersByTypeIdx = new()
		{
			{ TypeIndex.Type, SerializedTypeNameToType },
			{ TypeIndex.IXunitSerializer, DeserializeXunitSerializer },
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
			{ TypeIndex.Single, v => BitConverter.ToSingle((byte[])DeserializeArray(v), 0) },
			{ TypeIndex.Double, v => BitConverter.ToDouble((byte[])DeserializeArray(v), 0) },
			{ TypeIndex.Decimal, v => decimal.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Boolean, v => bool.Parse(v) },
			{ TypeIndex.DateTime, v => DateTime.Parse(v, CultureInfo.InvariantCulture, getDateStyle(v)) },
			{ TypeIndex.DateTimeOffset, v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture, getDateStyle(v)) },
			{ TypeIndex.TimeSpan, v => TimeSpan.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.BigInteger, v => BigInteger.Parse(v, CultureInfo.InvariantCulture) },
			{ TypeIndex.Version, Version.Parse },
			{ TypeIndex.Guid, v => Guid.Parse(v) },
			{ TypeIndex.Uri, v => new Uri(FromBase64(v), UriKind.RelativeOrAbsolute) },
		};

		if (dateOnlyFromDayNumber is not null)
			deserializersByTypeIdx.Add(TypeIndex.DateOnly, v => dateOnlyFromDayNumber.Invoke(null, [int.Parse(v, CultureInfo.InvariantCulture)]));
		if (timeOnlyCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.TimeOnly, v => timeOnlyCtor.Invoke([long.Parse(v, CultureInfo.InvariantCulture)]));
		if (indexCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.Index, v => DeserializeIndex(indexCtor, v));
		if (indexCtor is not null && rangeCtor is not null)
			deserializersByTypeIdx.Add(TypeIndex.Range, v => DeserializeRange(indexCtor, rangeCtor, v));

		serializersByTypeIdx = new()
		{
			{ TypeIndex.Type, (v, _) => TypeToSerializedTypeName((Type)v) },
			// No registration for IXunitSerializer, because we do a type-compatible search for the serializer
			// (inside Serialize) rather than a strong type matching
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
			{ TypeIndex.Single, (v, _) => SerializeArray(BitConverter.GetBytes((float)v)) },
			{ TypeIndex.Double, (v, _) => SerializeArray(BitConverter.GetBytes((double)v)) },
			{ TypeIndex.Decimal, (v, _) => ((decimal)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Boolean, (v, _) => v.ToString() ?? throw new InvalidOperationException("Boolean value returned null from ToString()") },
			{ TypeIndex.DateTime, (v, _) => ((DateTime)v).ToString("O", CultureInfo.InvariantCulture) },
			{ TypeIndex.DateTimeOffset, (v, _) => ((DateTimeOffset)v).ToString("O", CultureInfo.InvariantCulture) },
			{ TypeIndex.TimeSpan, (v, _) => ((TimeSpan)v).ToString("c", CultureInfo.InvariantCulture) },
			{ TypeIndex.BigInteger, (v, _) => ((BigInteger)v).ToString(CultureInfo.InvariantCulture) },
			{ TypeIndex.Version, (v, _) => ((Version)v).ToString() },
			{ TypeIndex.Guid, (v, _) => ((Guid)v).ToString("N") },
			{ TypeIndex.Uri, (v, _) => ToBase64(((Uri)v).OriginalString) },
		};

		if (dateOnlyDayNumber is not null)
			serializersByTypeIdx.Add(TypeIndex.DateOnly, (v, _) => dateOnlyDayNumber.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call GetValue on an instance of '{0}': {1}", dateOnlyType!.SafeName(), v)));
		if (timeOnlyTicks is not null)
			serializersByTypeIdx.Add(TypeIndex.TimeOnly, (v, _) => timeOnlyTicks.GetValue(v)?.ToString() ?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not call Ticks on an instance of '{0}': {1}", timeOnlyType!.SafeName(), v)));
		if (indexCtor is not null)
			serializersByTypeIdx.Add(TypeIndex.Index, (v, _) => v.ToString() ?? throw new InvalidOperationException("Index value returned null from ToString()"));
		if (rangeCtor is not null)
			serializersByTypeIdx.Add(TypeIndex.Range, (v, _) => v.ToString() ?? throw new InvalidOperationException("Range value returned null from ToString()"));

		xunitSeralizersByType = new()
		{
			{ typeof(IXunitSerializable), new XunitSerializableSerializer(this) },
			{ typeof(Enum), new EnumSerializer() },
		};

		if (FormattableAndParsableSerializer.IsSupported)
			xunitSeralizersByType.Add(typeof(IFormattable), new FormattableAndParsableSerializer());
	}

	/// <summary>
	/// Gets the singleton instance of <see cref="SerializationHelper"/>.
	/// </summary>
	public static SerializationHelper Instance { get; private set; } = new();

	/// <summary>
	/// Add serializers that have been registered in the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly to get registrations from</param>
	/// <param name="warnings">An optional collection to receive warnings generated during the registration</param>
	/// <remarks>
	/// The warnings collection will include warnings in the following circumstances:<br />
	/// * When the serializer type that does not implement <see cref="IXunitSerializer"/><br />
	/// * When the registration contains no support types to serialize<br />
	/// * When a supported type to serialize is duplicated with another serializer<br />
	/// * When a supported type is covered by a built-in serializer<br />
	/// * An exception is thrown while creating the serializer
	/// </remarks>
	public void AddRegisteredSerializers(
		Assembly assembly,
		List<string>? warnings = null)
	{
		Guard.ArgumentNotNull(assembly);

		var registrations =
			assembly
				.GetCustomAttributes()
				.OfType<IRegisterXunitSerializerAttribute>()
				.ToArray();

		AddSerializers(registrations, warnings);
	}

	/// <summary>
	/// Add serializers to the supported serializer list.
	/// </summary>
	/// <param name="registrations">The serialization registrations</param>
	/// <param name="warnings">An optional collection to receive warnings generated during the registration</param>
	/// <remarks>
	/// The warnings collection will include warnings in the following circumstances:<br />
	/// * When the serializer type that does not implement <see cref="IXunitSerializer"/><br />
	/// * When the registration contains no support types to serialize<br />
	/// * When a supported type to serialize is duplicated with another serializer<br />
	/// * When a supported type is covered by a built-in serializer<br />
	/// * An exception is thrown while creating the serializer
	/// </remarks>
	protected void AddSerializers(
		IRegisterXunitSerializerAttribute[] registrations,
		List<string>? warnings = null)
	{
		Guard.ArgumentNotNull(registrations);

		foreach (var registration in registrations)
		{
			try
			{
				if (registration.SupportedTypesForSerialization is null || registration.SupportedTypesForSerialization.Length == 0)
				{
					warnings?.Add(string.Format(
						CultureInfo.CurrentCulture,
						"Serializer type '{0}' does not have any supported types in the registration",
						registration.SerializerType.SafeName()
					));
					continue;
				}

				if (Activator.CreateInstance(registration.SerializerType) is not IXunitSerializer serializer)
				{
					warnings?.Add(string.Format(
						CultureInfo.CurrentCulture,
						"Serializer type '{0}' does not implement '{1}'",
						registration.SerializerType.SafeName(),
						typeof(IXunitSerializer).SafeName()
					));
					continue;
				}

				foreach (var supportedType in registration.SupportedTypesForSerialization)
				{
					if (xunitSeralizersByType.TryGetValue(supportedType, out var existingSerializer))
						warnings?.Add(string.Format(
							CultureInfo.CurrentCulture,
							"Serializer type '{0}' tried to register for type '{1}' which is already supported by serializer type '{2}'",
							registration.SerializerType.SafeName(),
							supportedType.SafeName(),
							existingSerializer.GetType().SafeName()
						));
					else if (IsSerializable(null, supportedType))
						warnings?.Add(string.Format(
							CultureInfo.CurrentCulture,
							"Serializer type '{0}' tried to register for type '{1}' which is supported by a built-in serializer",
							registration.SerializerType.SafeName(),
							supportedType.SafeName()
						));
					else
						xunitSeralizersByType[supportedType] = serializer;
				}
			}
			catch (Exception ex)
			{
				warnings?.Add(string.Format(
					CultureInfo.CurrentCulture,
					"Exception while creating serializer type '{0}': {1}",
					registration.SerializerType.SafeName(),
					ex
				));
			}
		}
	}

	/// <summary>
	/// De-serializes an object.
	/// </summary>
	/// <typeparam name="T">The type of the object</typeparam>
	/// <param name="serializedValue">The object's serialized value</param>
	/// <returns>The de-serialized object</returns>
	public T? Deserialize<T>(string serializedValue) =>
		(T?)Deserialize(serializedValue);

	/// <summary>
	/// De-serializes an object.
	/// </summary>
	/// <param name="serializedValue">The serialized value</param>
	/// <returns>The de-serialized object</returns>
	public object? Deserialize(string serializedValue)
	{
		Guard.ArgumentNotNull(serializedValue);

		var pieces = serializedValue.Split(colonSeparator, 2);
		var typeIdxText = pieces[0];

		if (typeIdxText.Equals("[]", StringComparison.Ordinal))
			return
				pieces.Length == 1
					? null
					: ArraySerializer.Deserialize(this, FromBase64(pieces[1]));

		if (!Enum.TryParse<TypeIndex>(typeIdxText, out var typeIdx) || typeIdx < TypeIndex_MinValue || typeIdx > TypeIndex_MaxValue)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Tried to deserialize unknown type index '{0}'", typeIdxText), nameof(serializedValue));

		if (!deserializersByTypeIdx.TryGetValue(typeIdx, out var deserializer))
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot deserialize value of '{0}': unsupported platform",
					typeIdx
				),
				nameof(serializedValue)
			);

		return
			pieces.Length == 1
				? null
				: deserializer(pieces[1]);
	}

	object DeserializeArray(string serializedValue) =>
		ArraySerializer.Deserialize(this, FromBase64(serializedValue));

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

	object? DeserializeXunitSerializer(string serializedValue)
	{
		var pieces = serializedValue.Split(colonSeparator, 2);
		if (pieces.Length != 2)
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"IXunitSerializer serialization '{0}' is malformed: expected two pieces, got one",
					serializedValue
				),
				nameof(serializedValue)
			);

		var type =
			SerializedTypeNameToType(FromBase64(pieces[0]))
				?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Serialized type name '{0}' could not be converted into a Type object.", pieces[0]));

		var xunitSerializer =
			FindXunitSerializer(type)
				?? throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Cannot deserialize value of type '{0}': no compatible serializer has been registered",
						type.SafeName()
					),
					nameof(serializedValue)
				);

		return xunitSerializer.Deserialize(type, FromBase64(pieces[1]));
	}

	static string FromBase64(string serializedValue)
	{
		var bytes = Convert.FromBase64String(serializedValue);
		return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	IXunitSerializer? FindXunitSerializer(Type type)
	{
		var inexactMatch = default(IXunitSerializer);

		foreach (var kvp in xunitSeralizersByType)
		{
			// Exact match always prevails
			if (kvp.Key == type)
				return kvp.Value;

			// Keep the first compatible match
			if (inexactMatch is null && kvp.Key.IsAssignableFrom(type))
				inexactMatch = kvp.Value;
		}

		return inexactMatch;
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
	public bool IsSerializable(object? value) =>
		value is null || IsSerializable(value, value.GetType());

	/// <summary>
	/// Determines if a given type supports serialization.
	/// </summary>
	/// <param name="value">The object to test for serializability.</param>
	/// <param name="type">The type to test for serializability.</param>
	/// <returns>Returns <c>true</c> if objects of the given type can be serialized; <c>false</c>, otherwise.</returns>
	public bool IsSerializable(
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
			if (!IsSerializable(value, type.GetElementType()))
				return false;

			// Then if we can, we want to verify every value in the array is okay
			if (value is Array valueArray)
				return valueArray.Cast<object?>().All(item => IsSerializable(item, item?.GetType()));
		}

		type = type.UnwrapNullable();
		return
			typeIndicesByType.ContainsKey(type) ||
			FindXunitSerializer(type)?.IsSerializable(type, value, out _) == true;
	}

	/// <summary>
	/// THIS METHOD IS FOR INTERNAL PURPOSES ONLY. DO NOT CALL.
	/// </summary>
	public static void ResetInstance() =>
		Instance = new();

	/// <summary>
	/// Serializes an object.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <param name="type">The type of the value to be serialized (cannot be <c>null</c> if <paramref name="value"/> is <c>null</c>)</param>
	/// <returns>The serialized object</returns>
	public string Serialize(
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
		var serializer = default(Func<object, Type, string>);

		TypeIndex typeIdx;
		if (typeof(Type).IsAssignableFrom(nonNullableCoreValueType))
			typeIdx = TypeIndex.Type;
		else
		{
			var kvp = typeIndicesByType.FirstOrDefault(kvp => nonNullableCoreValueType == kvp.Key);
			if (kvp.Key is not null)
				typeIdx = kvp.Value;
			else
			{
				var xunitSerializer =
					FindXunitSerializer(nonNullableCoreValueType)
						?? throw new ArgumentException(
							string.Format(
								CultureInfo.CurrentCulture,
								"Cannot serialize a value of type '{0}': unsupported type for serialization",
								type.SafeName()
							),
							nameof(value)
						);

				if (!xunitSerializer.IsSerializable(nonNullableCoreValueType, value, out var failureReason))
					throw new ArgumentException(failureReason, nameof(value));

				typeIdx = TypeIndex.IXunitSerializer;
				serializer = (v, t) => string.Format(CultureInfo.InvariantCulture, "{0}:{1}", ToBase64(TypeToSerializedTypeName(t)), ToBase64(xunitSerializer.Serialize(v)));
			}
		}

		if (serializer is null && !serializersByTypeIdx.TryGetValue(typeIdx, out serializer))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize a value of type '{0}': unsupported platform", typeIdx), nameof(value));

		var typeIdxText =
			isArray
				? "[]"
				: ((int)typeIdx).ToString(CultureInfo.InvariantCulture);

		return
			value is null
				? typeIdxText
				: isArray
					? string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, ToBase64(ArraySerializer.Serialize(this, coreValueType, (Array)value)))
					: typeIdx != TypeIndex.Object
						? string.Format(CultureInfo.InvariantCulture, "{0}:{1}", typeIdxText, serializer(value, nonNullableCoreValueType))
						: throw new ArgumentException("Cannot serialize a non-null value of type 'System.Object'", nameof(value));
	}

	string SerializeArray<T>(T[] array) =>
		ToBase64(ArraySerializer.Serialize(this, array));

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

	static string ToBase64(string value) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

	enum TypeIndex
	{
		// Special cases
		Type = -4,                // Custom serialization of the type name
		IXunitSerializer = -3,    // Supports any object with a registered IXunitSerializer
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
		Guid = 23,
		Uri = 24,
	}

	const TypeIndex TypeIndex_MinValue = TypeIndex.Type;
	const TypeIndex TypeIndex_MaxValue = TypeIndex.Uri;
}
