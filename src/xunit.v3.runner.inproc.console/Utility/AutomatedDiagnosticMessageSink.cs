#pragma warning disable CA2002  // The console writer is not cross app-domain

using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class AutomatedDiagnosticMessageSink(IRunnerLogger logger) :
	IMessageSink
{
	public bool OnMessage(IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(message);

		if (message is IDiagnosticMessage or IInternalDiagnosticMessage)
			logger.WriteMessage(message);

		return true;
	}
}
