using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Class that maps diagnostic messages to events.
	/// </summary>
	public class DiagnosticEventSink : LongLivedMarshalByRefObject, IMessageSinkWithTypes
	{
		/// <summary>
		/// Occurs when a <see cref="IDiagnosticMessage"/> message is received.
		/// </summary>
		public event MessageHandler<IDiagnosticMessage>? DiagnosticMessageEvent;

		/// <summary>
		/// Occurs when a <see cref="IErrorMessage"/> message is received.
		/// </summary>
		public event MessageHandler<IErrorMessage>? ErrorMessageEvent;

		/// <inheritdoc/>
		public void Dispose()
		{ }

		/// <inheritdoc/>
		public bool OnMessageWithTypes(
			IMessageSinkMessage message,
			HashSet<string>? typeNames)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			return message.Dispatch(typeNames, DiagnosticMessageEvent)
				&& message.Dispatch(typeNames, ErrorMessageEvent);
		}
	}
}
