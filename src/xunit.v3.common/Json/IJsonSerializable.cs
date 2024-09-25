namespace Xunit.Sdk;

/// <summary>
/// Interface that indicates an object can be serialized to JSON.
/// </summary>
public interface IJsonSerializable
{
	/// <summary>
	/// Converts the given object to JSON.
	/// </summary>
	/// <returns>
	/// Returns the object in JSON form, if possible; returns <c>null</c> if the object
	/// cannot be represented in JSON form.
	/// </returns>
	string? ToJson();
}
