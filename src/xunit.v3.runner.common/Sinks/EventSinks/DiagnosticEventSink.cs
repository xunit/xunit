using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
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
		/// Occurs when a <see cref="IErrorMessage"/> message is received.
		/// </summary>
		public event MessageHandler<IErrorMessage>? ErrorMessageEvent;

		/// <inheritdoc/>
		public bool OnMessage(IMessageSinkMessage message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return
				message.Dispatch(null, DiagnosticMessageEvent) &&
				message.Dispatch(null, ErrorMessageEvent);
		}
	}
}
