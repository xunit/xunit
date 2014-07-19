using System;
using System.Collections.Generic;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Utility classes for dealing with Exception objects.
    /// </summary>
    public static class ExceptionUtility
    {
        /// <summary>
        /// Combines multiple levels of messages into a single message.
        /// </summary>
        /// <param name="failureInfo">The failure information from which to get the messages.</param>
        /// <returns>The combined string.</returns>
        public static string CombineMessages(IFailureInformation failureInfo)
        {
            return GetMessage(failureInfo, 0, 0);
        }

        /// <summary>
        /// Combines multiple levels of stack traces into a single stack trace.
        /// </summary>
        /// <param name="failureInfo">The failure information from which to get the stack traces.</param>
        /// <returns>The combined string.</returns>
        public static string CombineStackTraces(IFailureInformation failureInfo)
        {
            return GetStackTrace(failureInfo, 0);
        }

        static bool ExcludeStackFrame(string stackFrame)
        {
            Guard.ArgumentNotNull("stackFrame", stackFrame);

            return stackFrame.StartsWith("at Xunit.", StringComparison.Ordinal);
        }

        static string FilterStackTrace(string stack)
        {
            if (stack == null)
                return null;

            var results = new List<string>();

            foreach (string line in SplitLines(stack))
            {
                string trimmedLine = line.TrimStart();
                if (!ExcludeStackFrame(trimmedLine))
                    results.Add(line);
            }

            return string.Join(Environment.NewLine, results.ToArray());
        }

        static string GetMessage(IFailureInformation failureInfo, int index, int level)
        {
            string result = "";

            if (level > 0)
            {
                for (int idx = 0; idx < level; idx++)
                    result += "----";

                result += " ";
            }

            var exceptionType = failureInfo.ExceptionTypes[index];
            if (GetNamespace(exceptionType) != "Xunit.Sdk")
                result += exceptionType + " : ";

            result += failureInfo.Messages[index];

            for (int subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
                if (failureInfo.ExceptionParentIndices[subIndex] == index)
                    result += Environment.NewLine + GetMessage(failureInfo, subIndex, level + 1);

            return result;
        }

        private static string GetNamespace(string exceptionType)
        {
            var nsIndex = exceptionType.LastIndexOf('.');
            if (nsIndex > 0)
                return exceptionType.Substring(0, nsIndex);

            return "";
        }

        static string GetStackTrace(IFailureInformation failureInfo, int index)
        {
            string result = FilterStackTrace(failureInfo.StackTraces[index]);

            var children = new List<int>();
            for (int subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
                if (failureInfo.ExceptionParentIndices[subIndex] == index)
                    children.Add(subIndex);

            if (children.Count > 1)
            {
                for (int idx = 0; idx < children.Count; ++idx)
                    result += String.Format("{0}----- Inner Stack Trace #{1} ({2}) -----{0}{3}",
                                            Environment.NewLine,
                                            idx + 1,
                                            failureInfo.ExceptionTypes[children[idx]],
                                            GetStackTrace(failureInfo, children[idx]));
            }
            else if (children.Count == 1)
                result += Environment.NewLine +
                          "----- Inner Stack Trace -----" + Environment.NewLine +
                          GetStackTrace(failureInfo, children[0]);

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

        /// <summary>
        /// Unwraps exceptions and their inner exceptions.
        /// </summary>
        /// <param name="ex">The exception to be converted.</param>
        /// <returns>The failure information.</returns>
        public static IFailureInformation ConvertExceptionToFailureInformation(Exception ex)
        {
            var exceptionTypes = new List<string>();
            var messages = new List<string>();
            var stackTraces = new List<string>();
            var indices = new List<int>();

            ConvertExceptionToFailureInformation(ex, -1, exceptionTypes, messages, stackTraces, indices);

            return new FailureInformation
            {
                ExceptionParentIndices = indices.ToArray(),
                ExceptionTypes = exceptionTypes.ToArray(),
                Messages = messages.ToArray(),
                StackTraces = stackTraces.ToArray(),
            };
        }

        static void ConvertExceptionToFailureInformation(Exception ex, int parentIndex, List<string> exceptionTypes, List<string> messages, List<string> stackTraces, List<int> indices)
        {
            var myIndex = exceptionTypes.Count;

            exceptionTypes.Add(ex.GetType().FullName);
            messages.Add(ex.Message);
            stackTraces.Add(ex.StackTrace);
            indices.Add(parentIndex);

#if XUNIT_CORE_DLL
            var aggEx = ex as AggregateException;
            if (aggEx != null)
                foreach (var innerException in aggEx.InnerExceptions)
                    ConvertExceptionToFailureInformation(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
            else
#endif
                if (ex.InnerException != null)
                    ConvertExceptionToFailureInformation(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
        }

        class FailureInformation : IFailureInformation
        {
            public string[] ExceptionTypes { get; set; }
            public string[] Messages { get; set; }
            public string[] StackTraces { get; set; }
            public int[] ExceptionParentIndices { get; set; }
        }

    }
}
