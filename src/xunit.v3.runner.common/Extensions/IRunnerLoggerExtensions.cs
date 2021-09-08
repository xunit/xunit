using Xunit.Internal;

namespace Xunit.Runner.Common
{
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(message), message);

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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogMessage(StackFrameInfo.None, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogMessage(stackFrame, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(message), message);

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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogImportantMessage(StackFrameInfo.None, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogImportantMessage(stackFrame, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(message), message);

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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogWarning(StackFrameInfo.None, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogWarning(stackFrame, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(message), message);

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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogError(StackFrameInfo.None, string.Format(messageFormat, args));
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
			Guard.ArgumentNotNull(nameof(logger), logger);
			Guard.ArgumentNotNull(nameof(messageFormat), messageFormat);

			logger.LogError(stackFrame, string.Format(messageFormat, args));
		}
	}
}
