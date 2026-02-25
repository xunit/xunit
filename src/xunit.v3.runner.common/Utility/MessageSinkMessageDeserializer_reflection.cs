using System.Reflection;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// A class which understands how to deserialize <see cref="IMessageSinkMessage"/> instances that are decorated
/// with <see cref="JsonTypeIDAttribute"/>. The built-in messages are registered by default, and additional messages
/// can be registered via <see cref="RegisterMessageSinkMessageType(string, Func{object?})"/>
/// or <see cref="RegisterMessageSinkMessageType(Type)"/>.
/// </summary>
partial class MessageSinkMessageDeserializer
{
	/// <summary>
	/// Registers an implementation of <see cref="IMessageSinkMessage"/> and <see cref="IJsonDeserializable"/>, decorated
	/// with <see cref="JsonTypeIDAttribute"/> so that it can be deserialized by the runner pipeline.
	/// </summary>
	/// <param name="type">The message type to register</param>
	public static void RegisterMessageSinkMessageType(Type type)
	{
		Guard.ArgumentNotNull(type);

		if (!typeof(IMessageSinkMessage).IsAssignableFrom(type))
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' must implement '{1}'", type.SafeName(), typeof(IMessageSinkMessage).SafeName()));
			return;
		}

		if (!typeof(IJsonDeserializable).IsAssignableFrom(type))
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' must implement '{1}'", type.SafeName(), typeof(IJsonDeserializable).SafeName()));
			return;
		}

		var attr = type.GetCustomAttribute<JsonTypeIDAttribute>();
		if (attr is null)
		{
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Message sink message type '{0}' is missing [JsonTypeID]", type.SafeName()));
			return;
		}

		if (!typeIDToFactoryMappings.TryAdd(attr.ID, () => Activator.CreateInstance(type)))
			errors.Add(string.Format(CultureInfo.CurrentCulture, "Could not add deserializer with JSON type ID of '{0}' because it's already registered", attr.ID));
	}
}
