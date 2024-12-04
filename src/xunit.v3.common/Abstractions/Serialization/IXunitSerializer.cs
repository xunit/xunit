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
	/// <remarks>
	/// This will be called by <see cref="SerializationHelper.IsSerializable(object?)"/>,
	/// <see cref="SerializationHelper.IsSerializable(object?, Type?)"/>, and
	/// <see cref="SerializationHelper.Serialize"/>. The failure reason is used when
	/// called from <c>Serialize</c> to format an error exception, but is otherwise ignored
	/// from the calls from <c>IsSerializable</c>.<br />
	/// <br />
	/// The type of <paramref name="value"/> may not directly match <paramref name="type"/>, as the type
	/// is derived from unwrapping nullability and array element types, so use care when looking
	/// at the value to determine serializability.
	/// </remarks>
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
	/// special cased by the serialization system. You may assume that <see cref="IsSerializable"/>
	/// is called before this, so any validation done there need not be repeated here.
	/// </remarks>
	string Serialize(object value);
}
