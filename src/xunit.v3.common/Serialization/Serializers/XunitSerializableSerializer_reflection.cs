namespace Xunit.Sdk;

internal sealed class XunitSerializableSerializer(SerializationHelper serializationHelper) :
	XunitSerializer<IXunitSerializable>
{
	/// <inheritdoc/>
	public override IXunitSerializable Deserialize(
		Type type,
		string serializedValue)
	{
		var serializationInfo = new XunitSerializationInfo(serializationHelper, serializedValue);

		try
		{
			if (Activator.CreateInstance(type) is not IXunitSerializable value)
				throw new ArgumentException(TypeIncompatibleDeserialization(type), nameof(type));

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
	public override string Serialize(IXunitSerializable value)
	{
		var info = new XunitSerializationInfo(serializationHelper);
		value.Serialize(info);
		return info.ToSerializedString();
	}
}
