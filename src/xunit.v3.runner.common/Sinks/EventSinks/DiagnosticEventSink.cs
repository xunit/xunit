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
	/// Occurs when a <see cref="IDiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<IDiagnosticMessage>? DiagnosticMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="IErrorMessage"/> message is received.
	/// </summary>
	public event MessageHandler<IErrorMessage>? ErrorMessageEvent;

	/// <summary>
	/// Occurs when a <see cref="IInternalDiagnosticMessage"/> message is received.
	/// </summary>
	public event MessageHandler<IInternalDiagnosticMessage>? InternalDiagnosticMessageEvent;

	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		return
			message.DispatchWhen(DiagnosticMessageEvent) &&
			message.DispatchWhen(ErrorMessageEvent) &&
			message.DispatchWhen(InternalDiagnosticMessageEvent);
	}
}
