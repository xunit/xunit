using Microsoft.Testing.Platform.Logging;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="IRunnerLogger"/> which forwards the messages onto
/// an implementation of <see cref="ILogger"/>.
/// </summary>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
public sealed class LoggerRunnerLogger(ILogger logger) :
	IRunnerLogger
{
	/// <inheritdoc/>
	public object LockObject { get; } = new();

	/// <inheritdoc/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message) =>
			logger.LogError(message);

	/// <inheritdoc/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message) =>
			logger.LogInformation(message);

	/// <inheritdoc/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message) =>
			logger.LogInformation(message);

	/// <inheritdoc/>
	public void LogRaw(string message) =>
		logger.LogInformation(message);

	/// <inheritdoc/>
	public void LogWarning(
		StackFrameInfo stackFrame,
		string message) =>
			logger.LogWarning(message);

	/// <inheritdoc/>
	public void WaitForAcknowledgment()
	{ }
}
