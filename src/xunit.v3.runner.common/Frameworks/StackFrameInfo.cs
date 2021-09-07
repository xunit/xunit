using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents the top of a stack frame, typically taken from an exception or failure information.
	/// </summary>
	public partial struct StackFrameInfo
	{
		readonly static Regex stackFrameRegex = GetStackFrameRegex();

		/// <summary>
		/// Initializes a new instance of the <see cref="StackFrameInfo"/> class.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="lineNumber"></param>
		public StackFrameInfo(
			string? fileName,
			int lineNumber)
		{
			FileName = fileName;
			LineNumber = lineNumber;
		}

		/// <summary>
		/// Gets the filename of the stack frame. May be <c>null</c> if the stack frame is not known.
		/// </summary>
		public string? FileName { get; }

		/// <summary>
		/// Returns <c>true</c> if this is an empty stack frame (e.g., <see cref="None"/>).
		/// </summary>
		public bool IsEmpty => string.IsNullOrEmpty(FileName) && LineNumber == 0;

		/// <summary>
		/// Gets the line number of the stack frame. May be 0 if the stack frame is not known.
		/// </summary>
		public int LineNumber { get; }

		/// <summary>
		/// Get a default (unknown) stack frame info.
		/// </summary>
		public static readonly StackFrameInfo None = new(null, 0);

		/// <summary>
		/// Creates a stack frame info from error metadata.
		/// </summary>
		/// <param name="errorMetadata">The error to inspect</param>
		/// <returns>The stack frame info</returns>
		public static StackFrameInfo FromErrorMetadata(_IErrorMetadata? errorMetadata)
		{
			if (errorMetadata == null)
				return None;

			var stackTraces = ExceptionUtility.CombineStackTraces(errorMetadata);
			if (stackTraces != null)
			{
				foreach (var frame in stackTraces.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries))
				{
					var match = stackFrameRegex.Match(frame);
					if (match.Success)
						return new StackFrameInfo(match.Groups["file"].Value, int.Parse(match.Groups["line"].Value));
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
				var getResourceStringMethod = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
				if (getResourceStringMethod != null)
				{
					wordAt = (string?)getResourceStringMethod.Invoke(null, new object[] { "Word_At" });
					wordsInLine = (string?)getResourceStringMethod.Invoke(null, new object[] { "StackTrace_InFileLineNumber" });
				}
			}
			catch { }  // Ignore failures that might be related to non-public reflection

			if (wordAt == default || wordAt == "Word_At")
				wordAt = "at";
			if (wordsInLine == default || wordsInLine == "StackTrace_InFileLineNumber")
				wordsInLine = "in {0}:line {1}";

			wordsInLine = wordsInLine.Replace("{0}", "(?<file>.*)").Replace("{1}", "(?<line>\\d+)");

			return new Regex($"{wordAt} .* {wordsInLine}");
		}
	}
}
