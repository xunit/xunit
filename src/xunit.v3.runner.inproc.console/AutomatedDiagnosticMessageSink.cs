#pragma warning disable CA2002  // The console writer is not cross app-domain

using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class AutomatedDiagnosticMessageSink : IMessageSink
{
	readonly ConsoleHelper consoleHelper;

	public AutomatedDiagnosticMessageSink(ConsoleHelper consoleHelper) =>
		this.consoleHelper = consoleHelper;

	public bool OnMessage(MessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is DiagnosticMessage || message is InternalDiagnosticMessage)
			lock (consoleHelper.LockObject)
				consoleHelper.WriteLine(message.ToJson());

		return true;
	}
}
