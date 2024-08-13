using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit.Runner.Common;

public class SpyRunnerLogger : IRunnerLogger
{
	static readonly string currentDirectory = Directory.GetCurrentDirectory();

	public List<string> Messages = [];

	public object LockObject { get; } = new();

	public void LogError(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Err", stackFrame, message);

	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Imp", stackFrame, message);

	public void LogMessage(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("---", stackFrame, message);

	public void LogRaw(string message) =>
		AddMessage("Raw", StackFrameInfo.None, message);

	public void LogWarning(
		StackFrameInfo stackFrame,
		string message) =>
			AddMessage("Wrn", stackFrame, message);

	public void WaitForAcknowledgment() =>
		AddMessage("Ack", StackFrameInfo.None, "Acknolwedgment requested");

	void AddMessage(
		string category,
		StackFrameInfo stackFrame,
		string message)
	{
		var result = new StringBuilder();
		result.Append($"[{category}");

		if (!stackFrame.IsEmpty)
		{
			var fileName = stackFrame.FileName;
			if (fileName?.StartsWith(currentDirectory) == true)
				fileName = fileName.Substring(currentDirectory.Length + 1);

			result.Append($" @ {fileName}:{stackFrame.LineNumber}");
		}

		result.Append($"] => {message}");

		Messages.Add(result.ToString());
	}
}
