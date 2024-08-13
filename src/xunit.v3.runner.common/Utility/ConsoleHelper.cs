// This code was adapted from https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/src/Build/Logging/BaseConsoleLogger.cs#L361-L401
// Under the MIT license https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/LICENSE

using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// This class helps write colored text to the console. On Windows, it will use the built-in
/// console functions; on Linux and macOS, it will use ANSI color codes.
/// </summary>
public class ConsoleHelper
{
	readonly TextReader consoleReader;
	readonly TextWriter consoleWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleHelper"/> class.
	/// </summary>
	/// <param name="consoleReader">The <see cref="TextReader"/> for the console (typically <see cref="Console.In"/>).</param>
	/// <param name="consoleWriter">The <see cref="TextWriter"/> for the console (typically <see cref="Console.Out"/>).</param>
	public ConsoleHelper(
		TextReader consoleReader,
		TextWriter consoleWriter)
	{
		this.consoleReader = Guard.ArgumentNotNull(consoleReader);
		this.consoleWriter = Guard.ArgumentNotNull(consoleWriter);

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			ResetColor = ResetColorConsole;
			SetBackgroundColor = SetBackgroundColorConsole;
			SetForegroundColor = SetForegroundColorConsole;
		}
		else
		{
			ResetColor = ResetColorANSI;
			SetBackgroundColor = SetBackgroundColorANSI;
			SetForegroundColor = SetForegroundColorANSI;
		}
	}

	/// <summary>
	/// Gets a lock object that can be used to lock multiple calls to <see cref="ConsoleHelper"/>
	/// functions to ensure consistent output.
	/// </summary>
	public object LockObject { get; } = new();

	/// <summary>
	/// Equivalent to <see cref="Console.ResetColor"/>.
	/// </summary>
	public Action ResetColor { get; private set; }

	/// <summary>
	/// Equivalent to <see cref="Console.BackgroundColor"/>.
	/// </summary>
	public Action<ConsoleColor> SetBackgroundColor { get; private set; }

	/// <summary>
	/// Equivalent to <see cref="Console.ForegroundColor"/>.
	/// </summary>
	public Action<ConsoleColor> SetForegroundColor { get; private set; }

	/// <summary>
	/// Gets a line of input from the console.
	/// </summary>
	public string? ReadLine() =>
		consoleReader.ReadLine();

	void ResetColorANSI() =>
		consoleWriter.Write("\x1b[0m");

	void ResetColorConsole() =>
		Console.ResetColor();

	void SetBackgroundColorANSI(ConsoleColor c)
	{
		var colorString = c switch
		{
			ConsoleColor.Black => "\x1b[40m",
			ConsoleColor.DarkBlue => "\x1b[44m",
			ConsoleColor.DarkGreen => "\x1b[42m",
			ConsoleColor.DarkCyan => "\x1b[46m",
			ConsoleColor.DarkRed => "\x1b[41m",
			ConsoleColor.DarkMagenta => "\x1b[45m",
			ConsoleColor.DarkYellow => "\x1b[43m",
			ConsoleColor.Gray => "\x1b[47m",
			ConsoleColor.DarkGray => "\x1b[100m",
			ConsoleColor.Blue => "\x1b[104m",
			ConsoleColor.Green => "\x1b[102m",
			ConsoleColor.Cyan => "\x1b[106m",
			ConsoleColor.Red => "\x1b[101m",
			ConsoleColor.Magenta => "\x1b[105m",
			ConsoleColor.Yellow => "\x1b[103m",
			ConsoleColor.White => "\x1b[107m",
			_ => "",
		};

		consoleWriter.Write(colorString);
	}

	void SetBackgroundColorConsole(ConsoleColor c) =>
		Console.BackgroundColor = c;

	void SetForegroundColorANSI(ConsoleColor c)
	{
		var colorString = c switch
		{
			ConsoleColor.Black => "\x1b[30m",
			ConsoleColor.DarkBlue => "\x1b[34m",
			ConsoleColor.DarkGreen => "\x1b[32m",
			ConsoleColor.DarkCyan => "\x1b[36m",
			ConsoleColor.DarkRed => "\x1b[31m",
			ConsoleColor.DarkMagenta => "\x1b[35m",
			ConsoleColor.DarkYellow => "\x1b[33m",
			ConsoleColor.Gray => "\x1b[37m",
			ConsoleColor.DarkGray => "\x1b[90m",
			ConsoleColor.Blue => "\x1b[94m",
			ConsoleColor.Green => "\x1b[92m",
			ConsoleColor.Cyan => "\x1b[96m",
			ConsoleColor.Red => "\x1b[91m",
			ConsoleColor.Magenta => "\x1b[95m",
			ConsoleColor.Yellow => "\x1b[93m",
			ConsoleColor.White => "\x1b[97m",
			_ => "",
		};

		consoleWriter.Write(colorString);
	}

	void SetForegroundColorConsole(ConsoleColor c) =>
		Console.ForegroundColor = c;

	/// <summary>
	/// Force using ANSI color instead of deciding based on OS.
	/// </summary>
	public void UseAnsiColor()
	{
		ResetColor = ResetColorANSI;
		SetBackgroundColor = SetBackgroundColorANSI;
		SetForegroundColor = SetForegroundColorANSI;
	}

	/// <summary>
	/// Writes the string value to the console.
	/// </summary>
	public void Write(string? value) =>
		consoleWriter.Write(value);

	/// <summary>
	/// Writes a formatted string value to the console.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	public void Write(string format, object? arg0) =>
		consoleWriter.Write(format, arg0);

	/// <summary>
	/// Writes a formatted string value to the console.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	/// <param name="arg1">The value to replace {1} in the format string</param>
	public void Write(string format, object? arg0, object? arg1) =>
		consoleWriter.Write(format, arg0, arg1);

	/// <summary>
	/// Writes a formatted string value to the console.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	/// <param name="arg1">The value to replace {1} in the format string</param>
	/// <param name="arg2">The value to replace {2} in the format string</param>
	public void Write(string format, object? arg0, object? arg1, object? arg2) =>
		consoleWriter.Write(format, arg0, arg1, arg2);

	/// <summary>
	/// Writes a formatted string value to the console.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public void Write(string format, params object?[] args) =>
		consoleWriter.Write(format, args);

	/// <summary>
	/// Writes <see cref="Environment.NewLine"/> to the console.
	/// </summary>
	public void WriteLine() =>
		consoleWriter.WriteLine();

	/// <summary>
	/// Writes the string value to the console, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	public void WriteLine(string? value) =>
		consoleWriter.WriteLine(value);

	/// <summary>
	/// Writes a formatted string value to the console, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	public void WriteLine(string format, object? arg0) =>
		consoleWriter.WriteLine(format, arg0);

	/// <summary>
	/// Writes a formatted string value to the console, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	/// <param name="arg1">The value to replace {1} in the format string</param>
	public void WriteLine(string format, object? arg0, object? arg1) =>
		consoleWriter.WriteLine(format, arg0, arg1);

	/// <summary>
	/// Writes a formatted string value to the console, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="arg0">The value to replace {0} in the format string</param>
	/// <param name="arg1">The value to replace {1} in the format string</param>
	/// <param name="arg2">The value to replace {2} in the format string</param>
	public void WriteLine(string format, object? arg0, object? arg1, object? arg2) =>
		consoleWriter.WriteLine(format, arg0, arg1, arg2);

	/// <summary>
	/// Writes a formatted string value to the console, followed by <see cref="Environment.NewLine"/>.
	/// </summary>
	/// <param name="format">The message format string</param>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public void WriteLine(string format, params object?[] args) =>
		consoleWriter.WriteLine(format, args);
}
