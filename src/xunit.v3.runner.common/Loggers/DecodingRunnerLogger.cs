using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerLogger"/> which is given to <c>ProjectAssemblyRunner.Run</c>,
/// which only uses <see cref="LogMessage"/> to send occasional JSON-encoded messages. This will decode
/// them and send them along to the correct message sink.
/// </summary>
public class DecodingRunnerLogger(
	IMessageSink messageSink,
	IMessageSink? diagnosticMessageSink) :
		IRunnerLogger
{
	/// <inheritdoc/>
	public object LockObject { get; } = new();

	/// <inheritdoc/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{ }

	/// <inheritdoc/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{ }

	/// <inheritdoc/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		var messageSinkMessage = MessageSinkMessageDeserializer.Deserialize(message, diagnosticMessageSink);
		if (messageSinkMessage is null)
			return;

		if (messageSinkMessage is IDiagnosticMessage or IInternalDiagnosticMessage)
			diagnosticMessageSink?.OnMessage(messageSinkMessage);
		else
			messageSink.OnMessage(messageSinkMessage);
	}

	/// <inheritdoc/>
	public void LogRaw(string message)
	{ }

	/// <inheritdoc/>
	public void LogWarning(
		StackFrameInfo stackFrame,
		string message)
	{ }

	/// <inheritdoc/>
	public void WaitForAcknowledgment()
	{ }
}
