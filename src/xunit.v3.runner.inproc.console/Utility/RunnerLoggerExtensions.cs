using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Extension methods for <see cref="IRunnerLogger"/>.
/// </summary>
public static class RunnerLoggerExtensions
{
	/// <summary>
	/// Writes a message to the console, and waits for acknowledge as appropriate.
	/// </summary>
	/// <param name="runnerLogger"></param>
	/// <param name="message">The message to write</param>
	public static void WriteMessage(
		this IRunnerLogger runnerLogger,
		IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(runnerLogger);
		Guard.ArgumentNotNull(message);

		runnerLogger.LogRaw(message.ToJson());
		runnerLogger.WaitForAcknowledgment();
	}
}
