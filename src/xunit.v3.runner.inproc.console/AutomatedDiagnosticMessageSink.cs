using System;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class AutomatedDiagnosticMessageSink : _IMessageSink
{
	readonly object consoleLock;

	public AutomatedDiagnosticMessageSink(object consoleLock) =>
		this.consoleLock = consoleLock;

	public bool OnMessage(_MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is _DiagnosticMessage || message is _InternalDiagnosticMessage)
			lock (consoleLock)
				Console.WriteLine(message.ToJson());

		return true;
	}
}
