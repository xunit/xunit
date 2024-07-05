using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerLogger"/> which logs messages to
/// a <see cref="TextWriter"/> (typically the one from <see cref="Console.Out"/>).
/// </summary>
public class ConsoleRunnerLogger : IRunnerLogger
{
	readonly static Regex ansiSgrRegex = new("\\e\\[\\d*(;\\d*)*m");
	readonly TextWriter consoleOutput;
	readonly bool useColors;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
	/// </summary>
	/// <param name="useColors">A flag to indicate whether colors should be used when
	/// logging messages.</param>
	/// <param name="useAnsiColor">A flag to indicate whether ANSI colors should be
	/// forced on Windows.</param>
	/// <param name="consoleOutput">The text writer for writing console output.</param>
	public ConsoleRunnerLogger(
		bool useColors,
		bool useAnsiColor,
		TextWriter consoleOutput)
	{
		Guard.ArgumentNotNull(consoleOutput);

		this.useColors = useColors;
		this.consoleOutput = Guard.ArgumentNotNull(consoleOutput);

		if (useAnsiColor)
			ConsoleHelper.UseAnsiColor();
	}

	/// <inheritdoc/>
	public object LockObject => consoleOutput;

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

	/// <summary>
	/// Writes a (non-colored) message. If <see cref="useColors"/> is <c>false</c>, all ANSI-SGR sequences will be
	/// removed prior to writing.
	/// </summary>
	/// <param name="message">Message to write</param>
	/// <remarks>See <see href="https://en.wikipedia.org/wiki/ANSI_escape_code#SGR" /> for details about ANSI-SGR.</remarks>
	public void WriteLine(string message)
	{
		Guard.ArgumentNotNull(message);

		var text = useColors ? message : RemoveAnsiSgr(message);
		consoleOutput.WriteLine(text);
	}

	static string RemoveAnsiSgr(string message) =>
		ansiSgrRegex.Replace(message, "");

	IDisposable? SetColor(ConsoleColor color) =>
		useColors ? new ColorRestorer(color) : null;

	sealed class ColorRestorer : IDisposable
	{
		public ColorRestorer(ConsoleColor color) =>
			ConsoleHelper.SetForegroundColor(color);

		public void Dispose() =>
			ConsoleHelper.ResetColor();
	}
}
