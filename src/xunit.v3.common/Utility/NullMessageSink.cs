namespace Xunit.Sdk;

/// <summary>
/// An implementation of <see cref="_IMessageSink"/> that ignores all messages.
/// </summary>
public class NullMessageSink : _IMessageSink
{
	NullMessageSink()
	{ }

	/// <summary>
	/// Gets the singleton null message sink instance.
	/// </summary>
	public static NullMessageSink Instance = new();

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message) => true;
}
