using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Utility class for dealing with <see cref="Exception"/> and <see cref="IFailureInformation"/> objects.
	/// </summary>
	public static class ExceptionUtility
	{
		/// <summary>
		/// Combines multiple levels of messages into a single message.
		/// </summary>
		/// <param name="failureInfo">The failure information from which to get the messages.</param>
		/// <returns>The combined string.</returns>
		public static string CombineMessages(IFailureInformation failureInfo) =>
			GetMessage(failureInfo, 0, 0);

		/// <summary>
		/// Combines multiple levels of stack traces into a single stack trace.
		/// </summary>
		/// <param name="failureInfo">The failure information from which to get the stack traces.</param>
		/// <returns>The combined string.</returns>
		public static string? CombineStackTraces(IFailureInformation failureInfo) =>
			GetStackTrace(failureInfo, 0);

		static bool ExcludeStackFrame(string stackFrame)
		{
			Guard.ArgumentNotNull("stackFrame", stackFrame);

#if DEBUG
			return false;
#else
			return stackFrame.StartsWith("at Xunit.", StringComparison.Ordinal);
#endif
		}

		static string? FilterStackTrace(string? stack)
		{
			if (stack == null)
				return null;

			var results = new List<string>();

			foreach (var line in stack.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
			{
				var trimmedLine = line.TrimStart();
				if (!ExcludeStackFrame(trimmedLine))
					results.Add(line);
			}

			return string.Join(Environment.NewLine, results.ToArray());
		}

		static string GetAt(
			string?[]? values,
			int index)
		{
			if (values == null || index < 0 || values.Length <= index)
				return string.Empty;

			return values[index] ?? string.Empty;
		}

		static int GetAt(
			int[]? values,
			int index)
		{
			if (values == null || values.Length <= index)
				return -1;

			return values[index];
		}

		static string GetMessage(
			IFailureInformation failureInfo,
			int index,
			int level)
		{
			Guard.ArgumentNotNull(nameof(failureInfo), failureInfo);

			var result = "";

			if (level > 0)
			{
				for (var idx = 0; idx < level; idx++)
					result += "----";

				result += " ";
			}

			var exceptionType = GetAt(failureInfo.ExceptionTypes, index);
			if (GetNamespace(exceptionType) != "Xunit.Sdk")
				result += exceptionType + " : ";

			result += GetAt(failureInfo.Messages, index);

			for (var subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
				if (GetAt(failureInfo.ExceptionParentIndices, subIndex) == index)
					result += Environment.NewLine + GetMessage(failureInfo, subIndex, level + 1);

			return result;
		}

		static string GetNamespace(string exceptionType)
		{
			var nsIndex = exceptionType.LastIndexOf('.');
			if (nsIndex > 0)
				return exceptionType.Substring(0, nsIndex);

			return "";
		}

		static string? GetStackTrace(
			IFailureInformation failureInfo,
			int index)
		{
			Guard.ArgumentNotNull(nameof(failureInfo), failureInfo);

			var result = FilterStackTrace(GetAt(failureInfo.StackTraces, index));

			var children = new List<int>();
			for (var subIndex = index + 1; subIndex < failureInfo.ExceptionParentIndices.Length; ++subIndex)
				if (GetAt(failureInfo.ExceptionParentIndices, subIndex) == index)
					children.Add(subIndex);

			if (children.Count > 1)
				for (var idx = 0; idx < children.Count; ++idx)
					result += $"{Environment.NewLine}----- Inner Stack Trace #{idx + 1} ({GetAt(failureInfo.ExceptionTypes, children[idx])}) -----{Environment.NewLine}{GetStackTrace(failureInfo, children[idx])}";
			else if (children.Count == 1)
				result += $"{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}{GetStackTrace(failureInfo, children[0])}";

			return result;
		}

		/// <summary>
		/// Unwraps exceptions and their inner exceptions.
		/// </summary>
		/// <param name="ex">The exception to be converted.</param>
		/// <returns>The error metadata.</returns>
		public static IFailureInformation ConvertExceptionToFailureInformation(Exception ex)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);

			var exceptionTypes = new List<string?>();
			var messages = new List<string>();
			var stackTraces = new List<string?>();
			var indices = new List<int>();

			ConvertExceptionToFailureInformation(ex, -1, exceptionTypes, messages, stackTraces, indices);

			return new FailureInformation(
				exceptionTypes.ToArray(),
				messages.ToArray(),
				stackTraces.ToArray(),
				indices.ToArray()
			);
		}

		static void ConvertExceptionToFailureInformation(
			Exception ex,
			int parentIndex,
			List<string?> exceptionTypes,
			List<string> messages,
			List<string?> stackTraces,
			List<int> indices)
		{
			var myIndex = exceptionTypes.Count;

			exceptionTypes.Add(ex.GetType().FullName);
			messages.Add(ex.Message);
			stackTraces.Add(ex.StackTrace);
			indices.Add(parentIndex);

			if (ex is AggregateException aggEx)
				foreach (var innerException in aggEx.InnerExceptions)
					ConvertExceptionToFailureInformation(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
			else if (ex.InnerException != null)
				ConvertExceptionToFailureInformation(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
		}

		class FailureInformation : IFailureInformation
		{
			public FailureInformation(
				string?[] exceptionTypes,
				string[] messages,
				string?[] stackTraces,
				int[] exceptionParentIndices)
			{
				ExceptionTypes = exceptionTypes;
				Messages = messages;
				StackTraces = stackTraces;
				ExceptionParentIndices = exceptionParentIndices;
			}

			public string?[] ExceptionTypes { get; set; }
			public string[] Messages { get; set; }
			public string?[] StackTraces { get; set; }
			public int[] ExceptionParentIndices { get; set; }
		}
	}
}
