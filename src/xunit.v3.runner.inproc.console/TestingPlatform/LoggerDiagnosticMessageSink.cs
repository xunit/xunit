using Microsoft.Testing.Platform.Logging;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IMessageSink"/> which forwards diagnostic messages to
/// an implementation of <see cref="ILogger"/>.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class LoggerDiagnosticMessageSink(
	ILogger logger,
	bool diagnosticMessages,
	bool internalDiagnosticMessages) :
		IMessageSink
{
	/// <inheritdoc/>
	public bool OnMessage(IMessageSinkMessage message)
	{
		if (diagnosticMessages && message is IDiagnosticMessage diagnosticMessage)
			logger.LogInformation(diagnosticMessage.Message);
		else if (internalDiagnosticMessages && message is IInternalDiagnosticMessage internalDiagnosticMessage)
			logger.LogInformation(internalDiagnosticMessage.Message);

		return true;
	}

	/// <summary>
	/// Factory function for the message sink
	/// </summary>
	public static IMessageSink? TryCreate(
		ILogger logger,
		bool diagnosticMessages,
		bool internalDiagnosticMessages) =>
			diagnosticMessages || internalDiagnosticMessages
				? new LoggerDiagnosticMessageSink(logger, diagnosticMessages, internalDiagnosticMessages)
				: null;
}
