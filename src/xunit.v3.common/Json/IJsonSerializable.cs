namespace Xunit.Sdk;

/// <summary>
/// Interface that indicates an object can be serialized to JSON.
/// </summary>
public interface IJsonSerializable
{
	/// <summary>
	/// Converts the given object to JSON.
	/// </summary>
	string ToJson();
}
