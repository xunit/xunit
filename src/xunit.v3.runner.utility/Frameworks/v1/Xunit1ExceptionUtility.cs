#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit.Internal;

namespace Xunit.Runner.v1;

static class Xunit1ExceptionUtility
{
	static readonly Regex NestedMessagesRegex = new(@"-*\s*((?<type>.*?) :\s*)?(?<message>.+?)((\r?\n-)|\z)", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);
	static readonly Regex NestedStackTracesRegex = new(@"\r?\n----- Inner Stack Trace -----\r?\n", RegexOptions.Compiled);

	public static (string?[] ExceptionTypes, string[] Messages, string?[] StackTraces, int[] ExceptionParentIndices) ConvertToErrorMetadata(Exception? exception)
	{
		var exceptionTypes = new List<string?>();
		var messages = new List<string>();
		var stackTraces = new List<string?>();
		var indices = new List<int>();
		var parentIndex = -1;

		while (exception is not null)
		{
			var stackTrace = exception.StackTrace;
			var rethrowIndex = stackTrace is null ? -1 : stackTrace.IndexOf("$$RethrowMarker$$", StringComparison.Ordinal);
			if (rethrowIndex > -1)
				stackTrace = stackTrace!.Substring(0, rethrowIndex);

			exceptionTypes.Add(exception.GetType().FullName);
			messages.Add(exception.Message);
			stackTraces.Add(stackTrace);
			indices.Add(parentIndex);

			parentIndex++;
			exception = exception.InnerException;
		}

		return (
			exceptionTypes.ToArray(),
			messages.ToArray(),
			stackTraces.ToArray(),
			indices.ToArray()
		);
	}

	public static (string?[] ExceptionTypes, string[] Messages, string?[] StackTraces, int[] ExceptionParentIndices) ConvertToErrorMetadata(XmlNode failureNode)
	{
		Guard.ArgumentNotNull(failureNode);

		var exceptionTypeAttribute = failureNode.Attributes?["exception-type"];
		var exceptionType = exceptionTypeAttribute is not null ? exceptionTypeAttribute.Value : string.Empty;
		var message = failureNode.SelectSingleNode("message")?.InnerText;
		var stackTraceNode = failureNode.SelectSingleNode("stack-trace");
		var stackTrace = stackTraceNode is null ? string.Empty : stackTraceNode.InnerText;

		return ConvertToErrorMetadata(exceptionType, message ?? "<unknown message>", stackTrace);
	}

	static (string?[] ExceptionTypes, string[] Messages, string?[] StackTraces, int[] ExceptionParentIndices) ConvertToErrorMetadata(
		string outermostExceptionType,
		string nestedExceptionMessages,
		string nestedStackTraces)
	{
		var exceptionTypes = new List<string?>();
		var messages = new List<string>();

		var match = NestedMessagesRegex.Match(nestedExceptionMessages);
		for (var i = 0; match.Success; i++, match = match.NextMatch())
		{
			exceptionTypes.Add(match.Groups["type"].Value);
			messages.Add(match.Groups["message"].Value);
		}

		if (exceptionTypes.Count > 0 && exceptionTypes[0]?.Length == 0)
			exceptionTypes[0] = outermostExceptionType;

		var stackTraces = NestedStackTracesRegex.Split(nestedStackTraces);
		var exceptionParentIndices = new int[stackTraces.Length];
		for (var i = 0; i < exceptionParentIndices.Length; i++)
			exceptionParentIndices[i] = i - 1;

		return (
			exceptionTypes.ToArray(),
			messages.ToArray(),
			stackTraces,
			exceptionParentIndices
		);
	}
}

#endif
