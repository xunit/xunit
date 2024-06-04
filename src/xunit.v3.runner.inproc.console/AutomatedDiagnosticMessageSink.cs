#pragma warning disable CA2002  // The console writer is not cross app-domain

using System.IO;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class AutomatedDiagnosticMessageSink : _IMessageSink
{
	readonly TextWriter consoleWriter;

	public AutomatedDiagnosticMessageSink(TextWriter consoleWriter) =>
		this.consoleWriter = consoleWriter;

	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is _DiagnosticMessage || message is _InternalDiagnosticMessage)
			lock (consoleWriter)
				consoleWriter.WriteLine(message.ToJson());

		return true;
	}
}
