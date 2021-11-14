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
		/// Occurs when a <see cref="_ErrorMessage"/> message is received.
		/// </summary>
		public event MessageHandler<_ErrorMessage>? ErrorMessageEvent;

		/// <inheritdoc/>
		public bool OnMessage(_MessageSinkMessage message)
		{
			Guard.ArgumentNotNull(message);

			return
				message.DispatchWhen(DiagnosticMessageEvent) &&
				message.DispatchWhen(ErrorMessageEvent);
		}
	}
}
