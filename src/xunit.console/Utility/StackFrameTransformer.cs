using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xunit.ConsoleClient
{
    /// <summary>
    /// Transforms stack frames and stack traces into compiler-like output
    /// so they can be double-clicked in Visual Studio.
    /// </summary>
    public static class StackFrameTransformer
    {
        static Regex regex;

        static StackFrameTransformer()
        {
            regex = new Regex(@"^\s*at (?<method>.*) in (?<file>.*):(line )?(?<line>\d+)$");
        }

        /// <summary/>
        public static string TransformFrame(string stackFrame, string defaultDirectory)
        {
            if (stackFrame == null)
                return null;

            var match = regex.Match(stackFrame);
            if (match == Match.Empty)
                return stackFrame;

            var file = match.Groups["file"].Value;
            if (file.StartsWith(defaultDirectory, StringComparison.OrdinalIgnoreCase))
                file = file.Substring(defaultDirectory.Length);

            return String.Format("{0}({1},0): at {2}",
                                 file,
                                 match.Groups["line"].Value,
                                 match.Groups["method"].Value);
        }

        /// <summary/>
        public static string TransformStack(string stack, string defaultDirectory)
        {
            if (stack == null)
                return null;

            List<string> results = new List<string>();

            foreach (string frame in stack.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                results.Add(TransformFrame(frame, defaultDirectory));

            return String.Join(Environment.NewLine, results.ToArray());
        }
    }
}
