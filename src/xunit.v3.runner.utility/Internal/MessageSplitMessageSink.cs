using Xunit.Sdk;

namespace Xunit.Internal;

// Splits diagnostic and non-diagnostic messages from a single source into separate sinks.

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class MessageSplitMessageSink(
	IMessageSink messageSink,
	IMessageSink? diagnosticMessageSink) :
		IMessageSink
{
	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (message is IDiagnosticMessage or IInternalDiagnosticMessage)
			return diagnosticMessageSink?.OnMessage(message) ?? true;

		return messageSink.OnMessage(message);
	}
}
