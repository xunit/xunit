#pragma warning disable CA1003 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps diagnostic messages to events.
/// </summary>
public class DiagnosticEventSink : IMessageSink
{
	/// <summary>
	/// Occurs when a <see cref="DiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<DiagnosticMessage>? DiagnosticMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="ErrorMessage"/> message is received.
	/// </summary>
	public event MessageHandler<ErrorMessage>? ErrorMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="InternalDiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<InternalDiagnosticMessage>? InternalDiagnosticMessageEvent;

	/// <inheritdoc/>
	public bool OnMessage(MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(DiagnosticMessageEvent) &&
			message.DispatchWhen(ErrorMessageEvent) &&
			message.DispatchWhen(InternalDiagnosticMessageEvent);
	}
}
