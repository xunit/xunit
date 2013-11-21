using System;
using System.Collections.Generic;

namespace Xunit.Sdk
{
    /// <summary>
    /// Utility methods for dealing with exceptions.
    /// </summary>
    public static class ExceptionUtility
    {
        static bool ExcludeStackFrame(string stackFrame)
        {
            Guard.ArgumentNotNull("stackFrame", stackFrame);

            return stackFrame.StartsWith("at Xunit.", StringComparison.Ordinal);
        }

        static string FilterStackTrace(string stack)
        {
            if (stack == null)
                return null;

            List<string> results = new List<string>();

            foreach (string line in SplitLines(stack))
            {
                string trimmedLine = line.TrimStart();
                if (!ExcludeStackFrame(trimmedLine))
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        /// <summary>
        /// Gets the message for the exception, including any inner exception messages.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>The formatted message</returns>
        public static string GetMessage(Exception ex)
        {
            return GetMessage(ex, 0);
        }

        static string GetMessage(Exception ex, int level)
        {
            string result = "";

            if (level > 0)
            {
                for (int idx = 0; idx < level; idx++)
                    result += "----";

                result += " ";
            }

            if (ex.GetType().Namespace != "Xunit.Sdk")
                result += ex.GetType().FullName + " : ";

            result += ex.Message;

            var aggEx = ex as AggregateException;
            if (aggEx != null)
            {
                foreach (var inner in aggEx.InnerExceptions)
                    result = result + Environment.NewLine + GetMessage(inner, level + 1);
            }
            else if (ex.InnerException != null)
                result = result + Environment.NewLine + GetMessage(ex.InnerException, level + 1);

            return result;
        }

        /// <summary>
        /// Gets the stack trace for the exception, including any inner exceptions.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>The formatted stack trace</returns>
        public static string GetStackTrace(Exception ex)
        {
            if (ex == null)
                return "";

            string result = FilterStackTrace(ex.StackTrace);

            var aggEx = ex as AggregateException;
            if (aggEx != null)
            {
                for (int idx = 0; idx < aggEx.InnerExceptions.Count; ++idx)
                    result += String.Format("{0}----- Inner Stack Trace #{1} ({2}) -----{0}{3}",
                                            Environment.NewLine,
                                            idx + 1,
                                            aggEx.InnerExceptions[idx].GetType().FullName,
                                            GetStackTrace(aggEx.InnerExceptions[idx]));
            }
            else if (ex.InnerException != null)
                result += Environment.NewLine +
                          "----- Inner Stack Trace -----" + Environment.NewLine +
                          GetStackTrace(ex.InnerException);

            return result;
        }

        // Our own custom String.Split because Silverlight/CoreCLR doesn't support the version we were using
        static IEnumerable<string> SplitLines(string input)
        {
            while (true)
            {
                int idx = input.IndexOf(Environment.NewLine);

                if (idx < 0)
                {
                    yield return input;
                    break;
                }

                yield return input.Substring(0, idx);
                input = input.Substring(idx + Environment.NewLine.Length);
            }
        }
    }
}
