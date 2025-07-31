using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;

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
		if (tupleType is null)
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
		Guard.NotNull(NotSupported, tupleItem);
		Guard.NotNull(NotSupported, tupleLength);

		try
		{
			var length =
				(int)(tupleLength.GetValue(value)
					?? throw new InvalidOperationException("Unexpected null calling ITuple.Length"));

			var values = new List<string>();

			for (var idx = 0; idx < length; ++idx)
				values.Add(SerializationHelper.Instance.Serialize(tupleItem.GetValue(value, [idx])));

			return string.Join("\n", values);
		}
		catch (Exception ex)
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Exception serializing tuple value of type '{0}': {1}", ArgumentFormatter.FormatTypeName(value.GetType()), ex.Message), nameof(value));
		}
	}

	static string TypeDoesNotHaveConstructor(Type type) =>
		string.Format(CultureInfo.CurrentCulture, "Tuple type '{0}' must have a constructor which accepts all the tuple values", ArgumentFormatter.FormatTypeName(type));

	static string TypeMustBeGeneric(Type type) =>
		string.Format(CultureInfo.CurrentCulture, "Tuple type '{0}' must be generic for serialization", ArgumentFormatter.FormatTypeName(type));
}
