using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

internal sealed class XunitSerializableSerializer(SerializationHelper serializationHelper) :
	IXunitSerializer
{
	/// <inheritdoc/>
	public object Deserialize(
		Type type,
		string serializedValue)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNullOrEmpty(serializedValue);

		if (!typeof(IXunitSerializable).IsAssignableFrom(type))
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot deserialize type '{0}' because it does not implement '{1}'.",
					type.SafeName(),
					typeof(IXunitSerializable).SafeName()
				),
				nameof(type)
			);

		var serializationInfo = new XunitSerializationInfo(serializationHelper, serializedValue);

		try
		{
			if (Activator.CreateInstance(type) is not IXunitSerializable value)
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Cannot deserialize type '{0}' because it does not implement '{1}'.",
						type.SafeName(),
						typeof(IXunitSerializable).SafeName()
					),
					nameof(type)
				);

			value.Deserialize(serializationInfo);
			return value;
		}
		catch (MissingMemberException)
		{
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Cannot deserialize type '{0}' because it lacks a parameterless constructor.",
					type.SafeName()
				),
				nameof(type)
			);
		}
	}

	/// <inheritdoc/>
	public bool IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (!typeof(IXunitSerializable).IsAssignableFrom(type))
		{
			failureReason = string.Format(
				CultureInfo.CurrentCulture,
				"Cannot serialize type '{0}' because it does not implement '{1}'",
				type.SafeName(),
				typeof(IXunitSerializable).SafeName()
			);
			return false;
		}

		failureReason = null;
		return true;
	}

	/// <inheritdoc/>
	public string Serialize(object value)
	{
		Guard.ArgumentNotNull(value);

		var info = new XunitSerializationInfo(serializationHelper);
		((IXunitSerializable)value).Serialize(info);
		return info.ToSerializedString();
	}
}
