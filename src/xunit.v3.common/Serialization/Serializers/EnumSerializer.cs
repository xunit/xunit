using System;
using System.Collections.Generic;
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
						"Attempted to deserialize type '{0}' which was not an enum",
						type.SafeName()
					),
					nameof(type)
				);

	/// <inheritdoc/>
	public bool IsSerializable(
		Type type,
		object? value)
	{
		Guard.ArgumentNotNull(type);

		return (type.IsEnum || type.IsNullableEnum()) && type.IsFromLocalAssembly();
	}

	/// <inheritdoc/>
	public string Serialize(object value)
	{
		Guard.ArgumentNotNull(value);

		var type = value.GetType();

		Guard.ArgumentValid(() => $"Attempted to serialize value of type '{type.SafeName()}' which is not Enum", value is Enum);

		if (!type.IsFromLocalAssembly())
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot serialize enum '{0}.{1}' because it lives in the GAC",
					type.SafeName(),
					value
				),
				nameof(value)
			);

		Type underlyingType;

		try
		{
			underlyingType = Enum.GetUnderlyingType(type);
		}
		catch (Exception ex)
		{
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot serialize enum '{0}.{1}' because an exception was thrown getting its underlying type",
					type.SafeName(),
					value
				),
				ex
			);
		}

		return
			!enumSignsByType.TryGetValue(underlyingType, out var signed)
				? throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Cannot serialize enum '{0}.{1}' because the underlying type '{2}' is not supported",
						type.SafeName(),
						value,
						underlyingType.SafeName()
					),
					nameof(value)
				)
				: signed
					? Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)
					: Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
	}
}
