// This code was adapted from https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/src/Build/Logging/BaseConsoleLogger.cs#L361-L401
// Under the MIT license https://github.com/Microsoft/msbuild/blob/ab090d1255caa87e742cbdbc6d7fe904ecebd975/LICENSE

using System;
using System.Runtime.InteropServices;

namespace Xunit.Runner.Common;

/// <summary>
/// This is a static class which helps write colored text to the console. On Windows, it will use the built-in
/// console functions; on Linux and macOS, it will use ANSI color codes.
/// </summary>
public static class ConsoleHelper
{
	/// <summary>
	/// Equivalent to <see cref="Console"/>.<see cref="Console.ResetColor"/>.
	/// </summary>
	public static Action ResetColor { get; }

	/// <summary>
	/// Equivalent to <see cref="Console"/>.<see cref="Console.BackgroundColor"/>.
	/// </summary>
	public static Action<ConsoleColor> SetBackgroundColor { get; }

	/// <summary>
	/// Equivalent to <see cref="Console"/>.<see cref="Console.ForegroundColor"/>.
	/// </summary>
	public static Action<ConsoleColor> SetForegroundColor { get; }

	static ConsoleHelper()
	{
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

	static void SetBackgroundColorANSI(ConsoleColor c)
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

		Console.Out.Write(colorString);
	}

	static void SetBackgroundColorConsole(ConsoleColor c) =>
		Console.BackgroundColor = c;

	static void SetForegroundColorANSI(ConsoleColor c)
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

		Console.Out.Write(colorString);
	}

	static void SetForegroundColorConsole(ConsoleColor c) =>
		Console.ForegroundColor = c;

	static void ResetColorANSI() =>
		Console.Out.Write("\x1b[0m");

	static void ResetColorConsole() =>
		Console.ResetColor();
}
