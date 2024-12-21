using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.MSBuild;

/// <summary/>
internal class MSBuildLogger(TaskLoggingHelper log) :
	IRunnerLogger
{
	/// <summary/>
	public object LockObject { get; private set; } = new object();

	/// <summary/>
	public TaskLoggingHelper Log { get; private set; } = log;

	/// <summary/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogError(null, null, null, stackFrame.FileName, stackFrame.LineNumber, 0, 0, 0, "{0}", message.Trim());
	}

	/// <summary/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogMessage(MessageImportance.High, "{0}", message);
	}

	/// <summary/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		Log.LogMessage("{0}", message);
	}

	/// <summary/>
	public void LogRaw(string message)
	{
		Guard.ArgumentNotNull(message);

		// We log with high importance, to make sure the message is always output.
		Log.LogMessage(MessageImportance.High, "{0}", message);
	}

	/// <summary/>
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

	/// <summary/>
	public void WaitForAcknowledgment()
	{ }
}
