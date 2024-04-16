using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The base type for all messages.
/// </summary>
public class _MessageSinkMessage
{
	string? type;
	static readonly ConcurrentDictionary<Type, string?> typeToTypeIDMappings = new();

	/// <summary>
	/// Initializes a new instance of the see <see cref="_MessageSinkMessage"/> class.
	/// </summary>
	public _MessageSinkMessage() =>
		type = typeToTypeIDMappings.GetOrAdd(GetType(), t => t.GetCustomAttribute<JsonTypeIDAttribute>()?.ID);

	internal virtual void Serialize(JsonObjectSerializer serializer)
	{
		ValidateObjectState();

		serializer.Serialize("Type", type);
	}

	/// <summary>
	/// Creates a JSON serialized version of this message.
	/// </summary>
	/// <returns>The serialization of this message, if serialization is supported; <c>null</c> otherwise.</returns>
	/// <exception cref="UnsetPropertiesException">Throw when one or more properties are missing values.</exception>
	public byte[]? ToJson()
	{
		if (type is null)
			return null;

		var buffer = new StringBuilder();
		using (var serializer = new JsonObjectSerializer(buffer))
			Serialize(serializer);

		return Encoding.UTF8.GetBytes(buffer.ToString());
	}

	/// <summary>
	/// Validates the state of the message object. This should be called just before serializing the message
	/// or just after deserializing the message to ensure that the message is not missing any required
	/// property values.
	/// </summary>
	/// <exception cref="UnsetPropertiesException">Throw when one or more properties are missing values.</exception>
	public void ValidateObjectState()
	{
		var invalidProperties = new HashSet<string>();

		ValidateObjectState(invalidProperties);

		if (invalidProperties.Count != 0)
			throw new UnsetPropertiesException(invalidProperties, GetType());
	}

	/// <summary>
	/// Called before serializing or just after deserializing the message. Implementers are expected
	/// to call <see cref="ValidatePropertyIsNotNull"/> for each property that must have a value,
	/// to record invalid property values into the provided hash set.
	/// </summary>
	/// <param name="invalidProperties">The hash set to record invalid properties into</param>
	protected virtual void ValidateObjectState(HashSet<string> invalidProperties)
	{ }

	/// <summary>
	/// Validates that the property value is not <c>null</c>, and if it is, adds the given
	/// property name to the invalid property hash set.
	/// </summary>
	/// <param name="propertyValue">The property value</param>
	/// <param name="propertyName">The property name</param>
	/// <param name="invalidProperties">The hash set to contain the invalid property name list</param>
	protected static void ValidatePropertyIsNotNull(
		object? propertyValue,
		string propertyName,
		HashSet<string> invalidProperties)
	{
		Guard.ArgumentNotNull(invalidProperties);

		if (propertyValue is null)
			invalidProperties.Add(propertyName);
	}
}
