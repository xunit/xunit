using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk;

/// <summary>
/// Implemented by types which can support serialization and deserialization. This
/// allows external serializers for types which would be inconvenient or impossible
/// to implement <see cref="IXunitSerializable"/> directly.
/// </summary>
public interface IXunitSerializer
{
	/// <summary>
	/// Deserializes a value that was obtained from <see cref="Serialize"/>.
	/// </summary>
	/// <param name="type">The type of the original value</param>
	/// <param name="serializedValue">The serialized value</param>
	/// <returns>The deserialized value</returns>
	object Deserialize(
		Type type,
		string serializedValue);

	/// <summary>
	/// Determines if a specific value of data is serializable.
	/// </summary>
	/// <param name="type">The type of the value</param>
	/// <param name="value">The value to test</param>
	/// <param name="failureReason">Returns a failure reason when the value isn't serializable</param>
	/// <returns>Return <c>true</c> if the value is serializable; <c>false</c>, otherwise</returns>
	bool IsSerializable(
		Type type,
		object? value,
		[NotNullWhen(false)] out string? failureReason);

	/// <summary>
	/// Serializes a value into a string to be later deserialized with <see cref="Deserialize"/>.
	/// </summary>
	/// <param name="value">The value to be serialized</param>
	/// <returns>The serialized value</returns>
	/// <remarks>
	/// This method will never be called with <c>null</c> values, because those are already
	/// special cased by the serialization system.
	/// </remarks>
	string Serialize(object value);
}
