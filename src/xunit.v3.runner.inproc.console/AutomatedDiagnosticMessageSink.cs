#pragma warning disable CA2002  // The console writer is not cross app-domain

using System.IO;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class AutomatedDiagnosticMessageSink : IMessageSink
{
	readonly TextWriter consoleWriter;

	public AutomatedDiagnosticMessageSink(TextWriter consoleWriter) =>
		this.consoleWriter = consoleWriter;

	public bool OnMessage(MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is DiagnosticMessage || message is InternalDiagnosticMessage)
			lock (consoleWriter)
				consoleWriter.WriteLine(message.ToJson());

		return true;
	}
}
