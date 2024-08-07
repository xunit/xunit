using System.Collections.Generic;

namespace Xunit.Runner.Common;

public class SpyRunnerLogger : IRunnerLogger
{
	public object LockObject => this;

	public List<string> Messages { get; } = [];

	void AddMessage(
		string type,
		string message,
		StackFrameInfo? stackFrame = null)
	{
		if (stackFrame.HasValue && !stackFrame.Value.IsEmpty)
			Messages.Add($"[{type} @ {stackFrame.Value.FileName}:{stackFrame.Value.LineNumber}] {message}");
		else
			Messages.Add($"[{type}] {message}");
	}

	public void LogError(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Err", message, stackFrame);

	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Imp", message, stackFrame);

	public void LogMessage(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Inf", message, stackFrame);

	public void LogRaw(string message) =>
		AddMessage("Raw", message);

	public void LogWarning(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Wrn", message, stackFrame);

	public void WaitForAcknowledgment()
	{ }
}
