using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="IDiagnosticMessage"/>.
	/// </summary>
	public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage, IMessageSinkMessageWithTypes
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
		/// </summary>
		public DiagnosticMessage()
			: this(string.Empty)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
		/// </summary>
		/// <param name="message">The message to send</param>
		public DiagnosticMessage(string message)
		{
			Guard.ArgumentNotNull(nameof(message), message);

			Message = message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
		/// </summary>
		/// <param name="format">The format of the message to send</param>
		/// <param name="args">The arguments used to format the message</param>
		public DiagnosticMessage(
			string format,
			params object?[] args)
		{
			Guard.ArgumentNotNull(nameof(format), format);

			Message = string.Format(format, args);
		}

		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(DiagnosticMessage).GetInterfaces().Select(x => x.FullName!));

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;

		/// <inheritdoc/>
		public string Message { get; }

		/// <inheritdoc/>
		public override string ToString() => Message;
	}
}
