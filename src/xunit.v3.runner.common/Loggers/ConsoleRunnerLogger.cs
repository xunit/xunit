using System;
using System.IO;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerLogger"/> which logs messages to
/// a <see cref="TextWriter"/> (typically the one from <see cref="Console.Out"/>).
/// </summary>
public class ConsoleRunnerLogger : IRunnerLogger
{
	readonly ConsoleHelper consoleHelper;
	readonly bool useColors;
	readonly bool waitForAcknowledgment;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
	/// </summary>
	/// <param name="useColors">A flag to indicate whether colors should be used when
	/// logging messages.</param>
	/// <param name="useAnsiColor">A flag to indicate whether ANSI colors should be
	/// forced on Windows.</param>
	/// <param name="consoleHelper">The helper for writing console output.</param>
	/// <param name="waitForAcknowledgment">A flag to indicate whether the logger should wait
	/// for acknowledgments or not</param>
	public ConsoleRunnerLogger(
		bool useColors,
		bool useAnsiColor,
		ConsoleHelper consoleHelper,
		bool waitForAcknowledgment)
	{
		this.useColors = useColors;
		this.consoleHelper = Guard.ArgumentNotNull(consoleHelper);
		this.waitForAcknowledgment = waitForAcknowledgment;

		if (useAnsiColor)
			consoleHelper.UseAnsiColor();
	}

	/// <inheritdoc/>
	public object LockObject => consoleHelper.LockObject;

	/// <inheritdoc/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Red))
				WriteLine(message);
	}

	/// <inheritdoc/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Gray))
				WriteLine(message);
	}

	/// <inheritdoc/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.DarkGray))
				WriteLine(message);
	}

	/// <inheritdoc/>
	public void LogRaw(string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			WriteLine(message);
	}

	/// <inheritdoc/>
	public void LogWarning(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Yellow))
				WriteLine(message);
	}

	/// <inheritdoc/>
	public void WaitForAcknowledgment()
	{
		if (waitForAcknowledgment)
			consoleHelper.ReadLine();
	}

	/// <summary>
	/// Writes a (non-colored) message. If <see cref="useColors"/> is <c>false</c>, all ANSI-SGR sequences will be
	/// removed prior to writing.
	/// </summary>
	/// <param name="message">Message to write</param>
	/// <remarks>See <see href="https://en.wikipedia.org/wiki/ANSI_escape_code#SGR" /> for details about ANSI-SGR.</remarks>
	public void WriteLine(string message)
	{
		Guard.ArgumentNotNull(message);

		var text = useColors ? message : AnsiUtility.RemoveAnsiEscapeCodes(message);
		consoleHelper.WriteLine(text);
	}

	IDisposable? SetColor(ConsoleColor color) =>
		useColors ? new ColorRestorer(consoleHelper, color) : null;

	sealed class ColorRestorer : IDisposable
	{
		readonly ConsoleHelper consoleHelper;

		public ColorRestorer(
			ConsoleHelper consoleHelper,
			ConsoleColor color)
		{
			this.consoleHelper = consoleHelper;

			consoleHelper.SetForegroundColor(color);
		}

		public void Dispose() =>
			consoleHelper.ResetColor();
	}
}
