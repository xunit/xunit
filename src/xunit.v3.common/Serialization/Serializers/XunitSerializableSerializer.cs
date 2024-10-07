using System;
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
					"Attempted to deserialize type '{0}' which does not implement '{1}'.",
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
						"Attempted to deserialize type '{0}' which does not implement '{1}'.",
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
					"Could not de-serialize type '{0}' because it lacks a parameterless constructor.",
					type.SafeName()
				),
				nameof(type)
			);
		}
	}

	/// <inheritdoc/>
	public bool IsSerializable(
		Type type,
		object? value) =>
			typeof(IXunitSerializable).IsAssignableFrom(type);

	/// <inheritdoc/>
	public string Serialize(object value)
	{
		Guard.ArgumentNotNull(value);

		if (value is not IXunitSerializable serializable)
			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Attempted to serialize value of type '{0}' which does not implement '{1}'.",
					value.GetType().SafeName(),
					typeof(IXunitSerializable).SafeName()
				),
				nameof(value)
			);

		var info = new XunitSerializationInfo(serializationHelper);
		serializable.Serialize(info);
		return info.ToSerializedString();
	}
}
