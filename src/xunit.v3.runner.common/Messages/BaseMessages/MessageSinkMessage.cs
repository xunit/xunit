using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Default implementation of <see cref="IMessageSinkMessage"/>, with serialization and deserialization support.
/// </summary>
/// <remarks>
/// Because of deserialization, all concrete message sink message types must have a parameterless public
/// constructor that will be used to create the message for deserialization purposes, and must be decorated
/// with <see cref="JsonTypeIDAttribute"/> to set a unique type ID for deserialization purposes.
/// </remarks>
public partial class MessageSinkMessage : IJsonDeserializable
{
	/// <summary>
	/// Empty traits, to be used to initialize traits values in messages.
	/// </summary>
	protected static IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();

	/// <summary>
	/// Gets the string value that message properties will return, when a value was not provided
	/// during deserialization.
	/// </summary>
	public const string UnsetStringPropertyValue = "<unset>";

	/// <summary>
	/// Override to deserialize the values in the dictionary into the message.
	/// </summary>
	/// <param name="root">The root of the JSON object</param>
	protected abstract void Deserialize(IReadOnlyDictionary<string, object?> root);

	/// <inheritdoc/>
	public void FromJson(IReadOnlyDictionary<string, object?> root) =>
		Deserialize(root);

	// We don't want to do object validation on this side, because these messages are meant to be
	// able to contain deserializations for both backward and forward compatible versions of the
	// message, so throwing when properties are missing isn't appropriate.
	static void ValidateObjectState()
	{ }
}
