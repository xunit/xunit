using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// A class which understands how to deserialize <see cref="IMessageSinkMessage"/> instances that are decorated
/// with <see cref="JsonTypeIDAttribute"/>. The built-in messages are registered by default, and additional messages
/// can be registered via <see cref="RegisterMessageSinkMessageType(string, Func{object?})"/>.
/// </summary>
partial class MessageSinkMessageDeserializer
{
	/// <summary>
	/// This overload is not available in Native AOT.
	/// Please call <see cref="RegisterMessageSinkMessageType(string, Func{object?})"/> instead.
	/// </summary>
	[Obsolete("This overload is not available in Native AOT. Please call the overload with jsonTypeID and factory.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void RegisterMessageSinkMessageType(Type type) =>
		throw new NotSupportedException();
}
