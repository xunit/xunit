using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

internal sealed class EnumSerializer : IXunitSerializer
{
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

	/// <inheritdoc/>
	public object Deserialize(
		Type type,
		string serializedValue) =>
			Guard.ArgumentNotNull(type).IsEnum
				? Enum.Parse(type, serializedValue)
				: throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Cannot deserialize type '{0}' because it is not Enum",
						type.SafeName()
					),
					nameof(type)
				);

	/// <inheritdoc/>
	public bool IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason)
	{
		Guard.ArgumentNotNull(type);

		if (TryGetEnumSign(type, value, out _) is string guardMessage)
		{
			failureReason = guardMessage;
			return false;
		}

		failureReason = null;
		return true;
	}

	/// <inheritdoc/>
	public string Serialize(object value)
	{
		Guard.ArgumentNotNull(value);

		if (TryGetEnumSign(value.GetType(), value, out var signed) is string guardMessage)
			throw new ArgumentException(guardMessage, nameof(value));

		return
			signed
				? Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
				: Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
	}

	static string? TryGetEnumSign(
		Type type,
		object? value,
		out bool signed)
	{
		signed = true;

		if (!type.IsEnum || type.IsNullableEnum())
			return string.Format(
				CultureInfo.CurrentCulture,
				"Cannot serialize type '{0}' because it is not Enum",
				type.SafeName()
			);

		if (!type.IsFromLocalAssembly())
			return string.Format(
				CultureInfo.CurrentCulture,
				"Cannot serialize enum of type '{0}' because it lives in the GAC",
				type.SafeName()
			);

		Type underlyingType;

		try
		{
			underlyingType = Enum.GetUnderlyingType(type);
		}
		catch (Exception ex)
		{
			return string.Format(
				CultureInfo.CurrentCulture,
				"Cannot serialize enum of type '{0}' because an exception was thrown getting its underlying type: {1}",
				type.SafeName(),
				ex
			);
		}

		if (!enumSignsByType.TryGetValue(underlyingType, out signed))
			return string.Format(
				CultureInfo.CurrentCulture,
				"Cannot serialize enum of type '{0}' because the underlying type '{1}' is not supported",
				type.SafeName(),
				underlyingType.SafeName()
			);

		return null;
	}
}
