using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Utility class for dealing with <see cref="Exception"/> and <see cref="_IErrorMetadata"/> objects.
	/// </summary>
	public static class ExceptionUtility
	{
		static ConcurrentDictionary<Type, MethodInfo?> innerExceptionsPropertyByType = new();

		/// <summary>
		/// Combines multiple levels of messages into a single message.
		/// </summary>
		/// <param name="errorMetadata">The error metadata from which to get the messages.</param>
		/// <returns>The combined string.</returns>
		public static string CombineMessages(_IErrorMetadata errorMetadata) =>
			GetMessage(errorMetadata, 0, 0);

		/// <summary>
		/// Combines multiple levels of stack traces into a single stack trace.
		/// </summary>
		/// <param name="errorMetadata">The error metadata from which to get the stack traces.</param>
		/// <returns>The combined string.</returns>
		public static string? CombineStackTraces(_IErrorMetadata errorMetadata) =>
			GetStackTrace(errorMetadata, 0);

		/// <summary>
		/// Unwraps exceptions and their inner exceptions.
		/// </summary>
		/// <param name="ex">The exception to be converted.</param>
		/// <returns>The error metadata.</returns>
		public static (string?[] ExceptionTypes, string[] Messages, string?[] StackTraces, int[] ExceptionParentIndices, FailureCause Cause) ExtractMetadata(Exception ex)
		{
			Guard.ArgumentNotNull(nameof(ex), ex);

			var exceptionTypes = new List<string?>();
			var messages = new List<string>();
			var stackTraces = new List<string?>();
			var indices = new List<int>();

			ExtractMetadata(ex, -1, exceptionTypes, messages, stackTraces, indices);

			var interfaces = ex.GetType().GetInterfaces();

			var cause =
				interfaces.Any(i => i.Name == "ITestTimeoutException")
					? FailureCause.Timeout
					: interfaces.Any(i => i.Name == "IAssertionException")
						? FailureCause.Assertion
						: FailureCause.Exception;

			return (
				exceptionTypes.ToArray(),
				messages.ToArray(),
				stackTraces.ToArray(),
				indices.ToArray(),
				cause
			);
		}

		static void ExtractMetadata(
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

			var innerExceptions = GetInnerExceptions(ex);

			if (innerExceptions != null)
				foreach (var innerException in innerExceptions)
					ExtractMetadata(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
			else if (ex.InnerException != null)
				ExtractMetadata(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
		}

		static IEnumerable<Exception>? GetInnerExceptions(Exception ex)
		{
			if (ex is AggregateException aggEx)
				return aggEx.InnerExceptions;

			var prop = innerExceptionsPropertyByType.GetOrAdd(
				ex.GetType(),
				t => t.GetProperties().FirstOrDefault(p => p.Name == "InnerExceptions" && p.CanRead)?.GetGetMethod()
			);

			return prop?.Invoke(ex, null) as IEnumerable<Exception>;
		}

		static bool FilterStackFrame(string stackFrame)
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
				if (!FilterStackFrame(trimmedLine))
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
			_IErrorMetadata errorMetadata,
			int index,
			int level)
		{
			Guard.ArgumentNotNull(nameof(errorMetadata), errorMetadata);

			var result = "";

			if (level > 0)
			{
				for (var idx = 0; idx < level; idx++)
					result += "----";

				result += " ";
			}

			var exceptionType = GetAt(errorMetadata.ExceptionTypes, index);
			if (GetNamespace(exceptionType) != "Xunit.Sdk")
				result += exceptionType + " : ";

			result += GetAt(errorMetadata.Messages, index);

			for (var subIndex = index + 1; subIndex < errorMetadata.ExceptionParentIndices.Length; ++subIndex)
				if (GetAt(errorMetadata.ExceptionParentIndices, subIndex) == index)
					result += Environment.NewLine + GetMessage(errorMetadata, subIndex, level + 1);

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
			_IErrorMetadata errorMetadata,
			int index)
		{
			Guard.ArgumentNotNull(nameof(errorMetadata), errorMetadata);

			var result = FilterStackTrace(GetAt(errorMetadata.StackTraces, index));

			var children = new List<int>();
			for (var subIndex = index + 1; subIndex < errorMetadata.ExceptionParentIndices.Length; ++subIndex)
				if (GetAt(errorMetadata.ExceptionParentIndices, subIndex) == index)
					children.Add(subIndex);

			if (children.Count > 1)
				for (var idx = 0; idx < children.Count; ++idx)
					result += $"{Environment.NewLine}----- Inner Stack Trace #{idx + 1} ({GetAt(errorMetadata.ExceptionTypes, children[idx])}) -----{Environment.NewLine}{GetStackTrace(errorMetadata, children[idx])}";
			else if (children.Count == 1)
				result += $"{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}{GetStackTrace(errorMetadata, children[0])}";

			return result;
		}
	}
}
