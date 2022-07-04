namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="_IMessageSink"/> that ignores all messages.
/// </summary>
public class _NullMessageSink : _IMessageSink
{
	_NullMessageSink()
	{ }

	/// <summary>
	/// Gets the singleton null message sink instance.
	/// </summary>
	public static _NullMessageSink Instance = new();

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message) => true;
}
