#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit.Abstractions;

namespace Xunit
{
    static class Xunit1ExceptionUtility
    {
        static readonly Regex NestedMessagesRegex = new Regex(@"-*\s*((?<type>.*?) :\s*)?(?<message>.+?)((\r\n-)|\z)", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);
        static readonly Regex NestedStackTracesRegex = new Regex(@"\r*\n----- Inner Stack Trace -----\r*\n", RegexOptions.Compiled);

        public static IFailureInformation ConvertToFailureInformation(Exception exception)
        {
            var exceptionTypes = new List<string>();
            var messages = new List<string>();
            var stackTraces = new List<string>();
            var indices = new List<int>();
            var parentIndex = -1;

            do
            {
                var stackTrace = exception.StackTrace;
                var rethrowIndex = stackTrace.IndexOf("$$RethrowMarker$$", StringComparison.Ordinal);
                if (rethrowIndex > -1)
                    stackTrace = stackTrace.Substring(0, rethrowIndex);

                exceptionTypes.Add(exception.GetType().FullName);
                messages.Add(exception.Message);
                stackTraces.Add(stackTrace);
                indices.Add(parentIndex);

                parentIndex++;
                exception = exception.InnerException;
            } while (exception != null);

            return new FailureInformation
            {
                ExceptionParentIndices = indices.ToArray(),
                ExceptionTypes = exceptionTypes.ToArray(),
                Messages = messages.ToArray(),
                StackTraces = stackTraces.ToArray(),
            };
        }

        public static IFailureInformation ConvertToFailureInformation(XmlNode failureNode)
        {
            var exceptionTypeAttribute = failureNode.Attributes["exception-type"];
            var exceptionType = exceptionTypeAttribute != null ? exceptionTypeAttribute.Value : string.Empty;
            var message = failureNode.SelectSingleNode("message").InnerText;
            var stackTraceNode = failureNode.SelectSingleNode("stack-trace");
            var stackTrace = stackTraceNode == null ? string.Empty : stackTraceNode.InnerText;

            return ConvertToFailureInformation(exceptionType, message, stackTrace);
        }

        static IFailureInformation ConvertToFailureInformation(string outermostExceptionType, string nestedExceptionMessages, string nestedStackTraces)
        {
            var failureInformation = new FailureInformation();

            var exceptionTypes = new List<string>();
            var messages = new List<string>();

            var match = NestedMessagesRegex.Match(nestedExceptionMessages);
            for (int i = 0; match.Success; i++, match = match.NextMatch())
            {
                exceptionTypes.Add(match.Groups["type"].Value);
                messages.Add(match.Groups["message"].Value);
            }

            if (exceptionTypes.Count > 0 && exceptionTypes[0] == "")
                exceptionTypes[0] = outermostExceptionType;

            failureInformation.ExceptionTypes = exceptionTypes.ToArray();
            failureInformation.Messages = messages.ToArray();
            failureInformation.StackTraces = NestedStackTracesRegex.Split(nestedStackTraces);

            failureInformation.ExceptionParentIndices = new int[failureInformation.StackTraces.Length];
            for (int i = 0; i < failureInformation.ExceptionParentIndices.Length; i++)
                failureInformation.ExceptionParentIndices[i] = i - 1;

            return failureInformation;
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

#endif
