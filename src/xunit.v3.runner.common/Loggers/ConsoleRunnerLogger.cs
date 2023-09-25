using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// An implementation of <see cref="IRunnerLogger"/> which logs messages
/// to <see cref="Console"/> and <see cref="Console.Error"/>.
/// </summary>
public class ConsoleRunnerLogger : IRunnerLogger
{
	readonly static Regex ansiSgrRegex = new Regex("\\e\\[\\d*(;\\d*)*m");
	readonly bool useColors;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
	/// </summary>
	/// <param name="useColors">A flag to indicate whether colors should be used when
	/// logging messages.</param>
	public ConsoleRunnerLogger(bool useColors)
		: this(useColors, new object())
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleRunnerLogger"/> class.
	/// </summary>
	/// <param name="useColors">A flag to indicate whether colors should be used when
	/// logging messages.</param>
	/// <param name="lockObject">The lock object used to prevent console clashes.</param>
	public ConsoleRunnerLogger(
		bool useColors,
		object lockObject)
	{
		Guard.ArgumentNotNull(lockObject);

		this.useColors = useColors;
		LockObject = lockObject;
	}

	/// <inheritdoc/>
	public object LockObject { get; }

	/// <inheritdoc/>
	public void LogError(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Red))
				WriteLine(Console.Error, message);
	}

	/// <inheritdoc/>
	public void LogImportantMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Gray))
				WriteLine(Console.Out, message);
	}

	/// <inheritdoc/>
	public void LogMessage(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.DarkGray))
				WriteLine(Console.Out, message);
	}

	/// <inheritdoc/>
	public void LogRaw(string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			WriteLine(Console.Out, message);
	}

	/// <inheritdoc/>
	public void LogWarning(
		StackFrameInfo stackFrame,
		string message)
	{
		Guard.ArgumentNotNull(message);

		lock (LockObject)
			using (SetColor(ConsoleColor.Yellow))
				WriteLine(Console.Out, message);
	}

	/// <summary>
	/// Writes a (non-colored) message. If <see cref="ConsoleRunnerLogger.useColors"/> is false, all ANSI-SGR sequences will be removed prior to writing.
	/// </summary>
	/// <param name="target">Target writer</param>
	/// <param name="message">Message to write</param>
	/// <remarks>See https://en.wikipedia.org/wiki/ANSI_escape_code#SGR for details about ANSI-SGR.</remarks>
	public void WriteLine(
		TextWriter target,
		string message)
	{
		Guard.ArgumentNotNull(target);
		Guard.ArgumentNotNull(message);

		var text = useColors ? message : RemoveAnsiSgr(message);
		target.WriteLine(text);
	}

	static string RemoveAnsiSgr(string message) =>
		ansiSgrRegex.Replace(message, "");

	IDisposable? SetColor(ConsoleColor color) =>
		useColors ? new ColorRestorer(color) : null;

	class ColorRestorer : IDisposable
	{
		public ColorRestorer(ConsoleColor color) =>
			ConsoleHelper.SetForegroundColor(color);

		public void Dispose() =>
			ConsoleHelper.ResetColor();
	}
}
