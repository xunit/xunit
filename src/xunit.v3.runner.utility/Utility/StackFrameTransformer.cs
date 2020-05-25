using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Xunit
{
    /// <summary>
    /// Transforms stack frames and stack traces into compiler-like output
    /// so they can be double-clicked in Visual Studio.
    /// </summary>
    public static class StackFrameTransformer
    {
        static readonly Regex regex;

        static StackFrameTransformer()
        {
            regex = new Regex(@"^\s*at (?<method>.*) in (?<file>.*):(line )?(?<line>\d+)$");
        }

        /// <summary>
        /// Transforms an individual stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame to transform</param>
        /// <param name="defaultDirectory">The default directory used for computing relative paths</param>
        /// <returns>The transformed stack frame</returns>
        public static string TransformFrame(string stackFrame, string defaultDirectory)
        {
            if (stackFrame == null)
                return null;

            var match = regex.Match(stackFrame);
            if (match == Match.Empty)
                return stackFrame;

            if (defaultDirectory != null)
                defaultDirectory = defaultDirectory.TrimEnd('\\', '/');

            var file = match.Groups["file"].Value;
            if (defaultDirectory != null && file.StartsWith(defaultDirectory, StringComparison.OrdinalIgnoreCase))
                file = file.Substring(defaultDirectory.Length + 1);

            return $"{file}({match.Groups["line"].Value},0): at {match.Groups["method"].Value}";
        }

        /// <summary>
        /// Transforms a stack.
        /// </summary>
        /// <param name="stack">The stack to transform</param>
        /// <param name="defaultDirectory">The default directory used for computing relative paths</param>
        /// <returns>The transformed stack</returns>
        public static string TransformStack(string stack, string defaultDirectory)
        {
            if (stack == null)
                return null;

            return string.Join(
                Environment.NewLine,
                stack.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                     .Select(frame => TransformFrame(frame, defaultDirectory))
                     .ToArray()
            );
        }
    }
}
