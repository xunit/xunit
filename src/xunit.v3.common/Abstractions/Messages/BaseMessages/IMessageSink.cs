namespace Xunit.Sdk;

/// <summary>
/// Represents an endpoint for the reception of test messages.
/// </summary>
public interface IMessageSink
{
	/// <summary>
	/// Reports the presence of a message on the message bus. This method should
	/// never throw exceptions.
	/// </summary>
	/// <param name="message">The message from the message bus</param>
	/// <returns>Return <see langword="true"/> to continue running tests, or <see langword="false"/> to stop.</returns>
	bool OnMessage(IMessageSinkMessage message);
}
