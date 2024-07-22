namespace Xunit.Sdk;

/// <summary>
/// An implementation of <see cref="IMessageSink"/> that ignores all messages.
/// </summary>
public class NullMessageSink : IMessageSink
{
	NullMessageSink()
	{ }

	/// <summary>
	/// Gets the singleton null message sink instance.
	/// </summary>
	public static NullMessageSink Instance = new();

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message) => true;
}
