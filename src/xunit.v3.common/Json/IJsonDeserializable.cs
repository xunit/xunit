using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Indicates that an object can be deserialized from string-serialized JSON.
/// </summary>
public interface IJsonDeserializable
{
	/// <summary>
	/// Deserializes the object's values from the provided JSON.
	/// </summary>
	/// <param name="root">The root of the deserialized JSON object</param>
	void FromJson(IReadOnlyDictionary<string, object?> root);
}
