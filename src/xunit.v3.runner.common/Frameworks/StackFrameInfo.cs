using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents the top of a stack frame, typically taken from an exception or failure information.
/// </summary>
/// <param name="fileName">The file name from the stack frame</param>
/// <param name="lineNumber">The line number from the stack frame</param>
public readonly partial struct StackFrameInfo(
	string? fileName,
	int lineNumber)
{
	static readonly Regex stackFrameRegex = GetStackFrameRegex();

	/// <summary>
	/// Gets the filename of the stack frame. May be <c>null</c> if the stack frame is not known.
	/// </summary>
	public string? FileName { get; } = fileName;

	/// <summary>
	/// Returns <c>true</c> if this is an empty stack frame (e.g., <see cref="None"/>).
	/// </summary>
	public bool IsEmpty => string.IsNullOrEmpty(FileName) && LineNumber == 0;

	/// <summary>
	/// Gets the line number of the stack frame. May be 0 if the stack frame is not known.
	/// </summary>
	public int LineNumber { get; } = lineNumber;

	/// <summary>
	/// Get a default (unknown) stack frame info.
	/// </summary>
	public static readonly StackFrameInfo None = new(null, 0);

	/// <summary>
	/// Creates a stack frame info from error metadata.
	/// </summary>
	/// <param name="errorMetadata">The error to inspect</param>
	/// <returns>The stack frame info</returns>
	public static StackFrameInfo FromErrorMetadata(IErrorMetadata? errorMetadata)
	{
		if (errorMetadata is null)
			return None;

		var stackTraces = ExceptionUtility.CombineStackTraces(errorMetadata);
		if (stackTraces is not null)
		{
			foreach (var frame in stackTraces.Split([Environment.NewLine], 2, StringSplitOptions.RemoveEmptyEntries))
			{
				var match = stackFrameRegex.Match(frame);
				if (match.Success)
					return new StackFrameInfo(match.Groups["file"].Value, int.Parse(match.Groups["line"].Value, CultureInfo.InvariantCulture));
			}
		}

		return None;
	}
	static Regex GetStackFrameRegex()
	{
		// Stack trace lines look like this:
		// "   at BooleanAssertsTests.False.AssertFalse() in c:\Dev\xunit\xunit\test\test.xunit.assert\Asserts\BooleanAssertsTests.cs:line 22"

		var wordAt = default(string);
		var wordsInLine = default(string);

		try
		{
			var getResourceStringMethod = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, [typeof(string)], null);
			if (getResourceStringMethod is not null)
			{
				wordAt = (string?)getResourceStringMethod.Invoke(null, ["Word_At"]);
				wordsInLine = (string?)getResourceStringMethod.Invoke(null, ["StackTrace_InFileLineNumber"]);
			}
		}
		catch { }  // Ignore failures that might be related to non-public reflection

		if (wordAt is null or "Word_At")
			wordAt = "at";
		if (wordsInLine is null or "StackTrace_InFileLineNumber")
			wordsInLine = "in {0}:line {1}";

		wordsInLine = wordsInLine.Replace("{0}", "(?<file>.*)").Replace("{1}", "(?<line>\\d+)");

		return new Regex(string.Format(CultureInfo.InvariantCulture, "{0} .* {1}", wordAt, wordsInLine));
	}
}
