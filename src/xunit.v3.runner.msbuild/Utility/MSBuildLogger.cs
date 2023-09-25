using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.MSBuild;

public class MSBuildLogger : IRunnerLogger
{
	public MSBuildLogger(TaskLoggingHelper log)
	{
		LockObject = new object();
		Log = log;
	}

	public object LockObject { get; private set; }

	public TaskLoggingHelper Log { get; private set; }

	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogError(null, null, null, stackFrame.FileName, stackFrame.LineNumber, 0, 0, 0, "{0}", message.Trim());
	}

	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogMessage(MessageImportance.High, "{0}", message);
	}

	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogMessage("{0}", message);
	}

	public void LogRaw(string message)
	{
		Guard.ArgumentNotNull(message);

		// We log with high importance, to make sure the message is always output.
		Log.LogMessage(MessageImportance.High, "{0}", message);
	}

	public void LogWarning(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		if (stackFrame.IsEmpty)
			Log.LogWarning("{0}", message.Trim());
		else
			Log.LogWarning(null, null, null, stackFrame.FileName, stackFrame.LineNumber, 0, 0, 0, "{0}", message.Trim());
	}
}
