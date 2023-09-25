#pragma warning disable CA1003 // The properties here are not intended to be .NET events

using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Class that maps diagnostic messages to events.
/// </summary>
public class DiagnosticEventSink : _IMessageSink
{
	/// <summary>
	/// Occurs when a <see cref="_DiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<_DiagnosticMessage>? DiagnosticMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="_ErrorMessage"/> message is received.
	/// </summary>
	public event MessageHandler<_ErrorMessage>? ErrorMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="_InternalDiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<_InternalDiagnosticMessage>? InternalDiagnosticMessageEvent;

	/// <inheritdoc/>
	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(DiagnosticMessageEvent) &&
			message.DispatchWhen(ErrorMessageEvent) &&
			message.DispatchWhen(InternalDiagnosticMessageEvent);
	}
}
