using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Extensions methods for <see cref="IRunnerLogger"/>.
/// </summary>
public static class IRunnerLoggerExtensions
{
	/// <summary>
	/// Logs a normal-priority message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="message">The message to be logged</param>
	public static void LogMessage(
		this IRunnerLogger logger,
		string message)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(message);

		logger.LogMessage(StackFrameInfo.None, message);
	}

	/// <summary>
	/// Logs a normal-priority formatted message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogMessage(
		this IRunnerLogger logger,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogMessage(StackFrameInfo.None, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a normal-priority formatted message with stack frame.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="stackFrame">The stack frame information</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogMessage(
		this IRunnerLogger logger,
		StackFrameInfo stackFrame,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogMessage(stackFrame, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a high-priority message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="message">The message to be logged</param>
	public static void LogImportantMessage(
		this IRunnerLogger logger,
		string message)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(message);

		logger.LogImportantMessage(StackFrameInfo.None, message);
	}

	/// <summary>
	/// Logs a high-priority formatted message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogImportantMessage(
		this IRunnerLogger logger,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogImportantMessage(StackFrameInfo.None, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a high-priority formatted message with stack frame.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="stackFrame">The stack frame information</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogImportantMessage(
		this IRunnerLogger logger,
		StackFrameInfo stackFrame,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogImportantMessage(stackFrame, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a warning message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="message">The message to be logged</param>
	public static void LogWarning(
		this IRunnerLogger logger,
		string message)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(message);

		logger.LogWarning(StackFrameInfo.None, message);
	}

	/// <summary>
	/// Logs a formatted warning message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogWarning(
		this IRunnerLogger logger,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogWarning(StackFrameInfo.None, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a formatted warning message with stack frame.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="stackFrame">The stack frame information</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogWarning(
		this IRunnerLogger logger,
		StackFrameInfo stackFrame,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogWarning(stackFrame, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs an error message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="message">The message to be logged</param>
	public static void LogError(
		this IRunnerLogger logger,
		string message)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(message);

		logger.LogError(StackFrameInfo.None, message);
	}

	/// <summary>
	/// Logs a formatted error message.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogError(
		this IRunnerLogger logger,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogError(StackFrameInfo.None, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a formatted error message with stack frame.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="stackFrame">The stack frame information</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogError(
		this IRunnerLogger logger,
		StackFrameInfo stackFrame,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		logger.LogError(stackFrame, string.Format(CultureInfo.CurrentCulture, messageFormat, args));
	}

	/// <summary>
	/// Logs a messages with as little processing as possible. For example, the console runner will
	/// not attempt to set the color of the text that's being logged. This is most useful when attempting
	/// to render text lines that will be processed, like for TeamCity.
	/// </summary>
	/// <param name="logger">The logger</param>
	/// <param name="messageFormat">The format of the message to be logged</param>
	/// <param name="args">The format arguments</param>
	public static void LogRaw(
		this IRunnerLogger logger,
		string messageFormat,
		params object?[] args)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(messageFormat);

		// Use InvariantCulture here since this is the output for things which may be later parsed
		logger.LogRaw(string.Format(CultureInfo.InvariantCulture, messageFormat, args));
	}

	/// <summary>
	/// Writes a message to the console, and waits for acknowledgement as appropriate.
	/// </summary>
	/// <remarks>
	/// This method should only be called when you are known to be using a JSON stream output; otherwise, it
	/// will emit JSON into an otherwise human-readable output.
	/// </remarks>
	/// <param name="logger">The logger</param>
	/// <param name="message">The message to write</param>
	public static void WriteMessageJson(
		this IRunnerLogger logger,
		IMessageSinkMessage message)
	{
		Guard.ArgumentNotNull(logger);
		Guard.ArgumentNotNull(message);

		var json = message.ToJson();
		if (json is not null)
		{
			logger.LogRaw(json);
			logger.WaitForAcknowledgment();
		}
	}
}
