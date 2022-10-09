namespace Xunit.Sdk;

/// <summary>
/// This interface should be implemented by any custom object which requires serialization.
/// In addition to implementing this interface, objects must also offer a parameterless
/// public constructor so that an empty object can be created to be deserialized into.
/// </summary>
public interface IXunitSerializable
{
	/// <summary>
	/// Called when the object should populate itself with data from the serialization info.
	/// </summary>
	/// <param name="info">The info to get the object data from</param>
	void Deserialize(IXunitSerializationInfo info);

	/// <summary>
	/// Called when the object should store its serialized values into the serialization info.
	/// </summary>
	/// <param name="info">The info to store the object data into</param>
	void Serialize(IXunitSerializationInfo info);
}
