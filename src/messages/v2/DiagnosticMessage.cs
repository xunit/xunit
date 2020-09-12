using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.v2
#else
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Xunit.Runner.v2
#endif
{
	/// <summary>
	/// Default implementation of <see cref="IDiagnosticMessage"/>.
	/// </summary>
#if XUNIT_FRAMEWORK
	public class DiagnosticMessage : IDiagnosticMessage
#else
	public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage, IMessageSinkMessageWithTypes
#endif
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

#if !XUNIT_FRAMEWORK
		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(DiagnosticMessage).GetInterfaces().Select(x => x.FullName!));

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;
#endif

		/// <inheritdoc/>
		public string Message { get; }

		/// <inheritdoc />
		public override string ToString() => Message;
	}
}
