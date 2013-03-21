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
            regex = new Regex(@"^(?<spaces>\s*)at (?<method>.*) in (?<file>.*):(line )?(?<line>\d+)$");
        }

        /// <summary/>
        public static string TransformFrame(string stackFrame)
        {
            if (stackFrame == null)
                return null;

            Match match = regex.Match(stackFrame);
            if (match == Match.Empty)
                return stackFrame;

            return String.Format("{0}{1}({2},0): at {3}",
                                 match.Groups["spaces"].Value,
                                 match.Groups["file"].Value,
                                 match.Groups["line"].Value,
                                 match.Groups["method"].Value);
        }

        /// <summary/>
        public static string TransformStack(string stack)
        {
            if (stack == null)
                return null;

            List<string> results = new List<string>();

            foreach (string frame in stack.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                results.Add(TransformFrame(frame));

            return String.Join(Environment.NewLine, results.ToArray());
        }
    }
}
