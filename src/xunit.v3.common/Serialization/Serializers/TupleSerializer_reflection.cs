using System.Reflection;

namespace Xunit.Sdk;

internal sealed class TupleSerializer : IXunitSerializer
{
	const string NotSupported = "Type System.Runtime.CompilerServices.ITuple is not supported on the current platform";

	static readonly Type? tupleType = Type.GetType("System.Runtime.CompilerServices.ITuple");
	static readonly PropertyInfo? tupleItem = tupleType?.GetRuntimeProperty("Item");
	static readonly PropertyInfo? tupleLength = tupleType?.GetRuntimeProperty("Length");

	public static bool IsSupported =>
		tupleType is not null && tupleItem is not null && tupleLength is not null;

	public static Type TupleType =>
		tupleType ?? throw new InvalidOperationException(NotSupported);

	public object Deserialize(
		Type type,
		string serializedValue)
	{
		if (!type.IsGenericType)
			throw new ArgumentException(TypeMustBeGeneric(type), nameof(type));

		var ctor =
			type.GetConstructor(type.GenericTypeArguments)
				?? throw new ArgumentException(TypeDoesNotHaveConstructor(type), nameof(type));

		var values = serializedValue.Split('\n').Select(SerializationHelper.Instance.Deserialize).ToArray();
		return ctor.Invoke(values);
	}

	public bool IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (tupleType is null || tupleItem is null || tupleLength is null)
		{
			failureReason = NotSupported;
			return false;
		}

		if (!type.IsGenericType)
		{
			failureReason = TypeMustBeGeneric(type);
			return false;
		}

		if (type.GetConstructor(type.GenericTypeArguments) is null)
		{
			failureReason = TypeDoesNotHaveConstructor(type);
			return false;
		}

		failureReason = null;
		return true;
	}

	public string Serialize(object value)
	{
		try
		{
			return Serialize(value, value.GetType(), 0);
		}
		catch (Exception ex)
		{
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Exception serializing tuple value of type '{0}': {1}",
					ArgumentFormatter.FormatTypeName(value.GetType()),
					ex.Message ?? "(null message)"
				),
				nameof(value)
			);
		}
	}

	static string Serialize(
		object tuple,
		Type type,
		int startIndex)
	{
		if (!type.IsGenericType)
			throw new ArgumentException(TypeMustBeGeneric(type), nameof(tuple));

		if (!tupleType!.IsAssignableFrom(type))
			throw new ArgumentException(TypeIsNotTuple(type, tupleType), nameof(tuple));

		var argumentTypes = type.GenericTypeArguments;
		var values = new List<string>();

		for (var idx = 0; idx < argumentTypes.Length; ++idx)
			values.Add(
				tupleType.IsAssignableFrom(argumentTypes[idx])
					? SerializationHelper.SerializeForXunitSerializer(argumentTypes[idx], Serialize(tuple, argumentTypes[idx], startIndex + idx))
					: SerializationHelper.Instance.Serialize(tupleItem?.GetValue(tuple, [startIndex + idx]))
			);

		return string.Join("\n", values);
	}

	static string TypeDoesNotHaveConstructor(Type type) =>
		string.Format(CultureInfo.CurrentCulture, "Tuple type '{0}' must have a constructor which accepts all the tuple values", ArgumentFormatter.FormatTypeName(type));

	static string TypeIsNotTuple(
		Type type,
		Type tupleType) =>
			string.Format(CultureInfo.CurrentCulture, "Type '{0}' does not implement tuple interface '{1}'", ArgumentFormatter.FormatTypeName(type), ArgumentFormatter.FormatTypeName(tupleType));

	static string TypeMustBeGeneric(Type type) =>
		string.Format(CultureInfo.CurrentCulture, "Tuple type '{0}' must be generic for serialization", ArgumentFormatter.FormatTypeName(type));
}
