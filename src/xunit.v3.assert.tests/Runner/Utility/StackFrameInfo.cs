using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Represents the top of a stack frame, typically taken from an exception or failure information.
    /// </summary>
    public struct StackFrameInfo
    {
        readonly static Regex stackFrameRegex = GetStackFrameRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="StackFrameInfo"/> class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lineNumber"></param>
        public StackFrameInfo(string fileName, int lineNumber)
        {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets the filename of the stack frame. May be <c>null</c> if the stack frame is not known.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if this is an empty stack frame (e.g., <see cref="None"/>).
        /// </summary>
        public bool IsEmpty { get { return string.IsNullOrEmpty(FileName) && LineNumber == 0; } }

        /// <summary>
        /// Gets the line number of the stack frame. May be 0 if the stack frame is not known.
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// Get a default (unknown) stack frame info.
        /// </summary>
        public static readonly StackFrameInfo None = new StackFrameInfo(null, 0);

        /// <summary>
        /// Creates a stack frame info from failure information.
        /// </summary>
        /// <param name="failureInfo">The failure information to inspect</param>
        /// <returns>The stack frame info</returns>
        public static StackFrameInfo FromFailure(IFailureInformation failureInfo)
        {
            if (failureInfo == null)
                return None;

            var stackTraces = ExceptionUtility.CombineStackTraces(failureInfo);
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

        /// <summary>
        /// Creates a tack frame from source information. This can be useful when simulating a
        /// stack frame in a non-exceptional situation (f.e., for a skipped test).
        /// </summary>
        /// <param name="sourceInfo">The source information to inspect</param>
        /// <returns>The stack frame info</returns>
        public static StackFrameInfo FromSourceInformation(ISourceInformation sourceInfo)
        {
            if (sourceInfo == null)
                return None;

            return new StackFrameInfo(sourceInfo.FileName, sourceInfo.LineNumber ?? 0);
        }

        static Regex GetStackFrameRegex()
        {
            // Stack trace lines look like this:
            // "   at BooleanAssertsTests.False.AssertFalse() in c:\Dev\xunit\xunit\test\test.xunit.assert\Asserts\BooleanAssertsTests.cs:line 22"

            var wordAt = default(string);
            var wordsInLine = default(string);

#if NETFRAMEWORK
            var getResourceStringMethod = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
            if (getResourceStringMethod != null)
            {
                wordAt = (string)getResourceStringMethod.Invoke(null, new object[] { "Word_At" });
                wordsInLine = (string)getResourceStringMethod.Invoke(null, new object[] { "StackTrace_InFileLineNumber" });
            }
#endif

            if (wordAt == default || wordAt == "Word_At")
                wordAt = "at";
            if (wordsInLine == default || wordsInLine == "StackTrace_InFileLineNumber")
                wordsInLine = "in {0}:line {1}";

            wordsInLine = wordsInLine.Replace("{0}", "(?<file>.*)").Replace("{1}", "(?<line>\\d+)");

            return new Regex($"{wordAt} .* {wordsInLine}");
        }
    }
}
