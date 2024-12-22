using System.Text.RegularExpressions;

namespace Xunit.Sdk;

/// <summary>
/// A utility class for ANSI color escape codes.
/// </summary>
public static class AnsiUtility
{
	/// <summary>
	/// Gets a regular expression that can used to find ANSI color escape codes.
	/// </summary>
	public static Regex AnsiEscapeCodeRegex { get; } = new("\\e\\[\\d*(;\\d*)*m");

	/// <summary>
	/// Strip ANSI color escape codes (in the form of <c>ESC[1;2m</c>) from a string value.
	/// </summary>
	/// <param name="message">The message that may contain ANSI color escape codes</param>
	/// <returns>The message without the ANSI color escape codes</returns>
	public static string RemoveAnsiEscapeCodes(string message) =>
		AnsiEscapeCodeRegex.Replace(message, "");
}
