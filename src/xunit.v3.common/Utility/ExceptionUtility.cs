using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Utility class for dealing with <see cref="Exception"/> and <see cref="IErrorMetadata"/> objects.
/// </summary>
public static class ExceptionUtility
{
	static readonly ConcurrentDictionary<Type, MethodInfo?> innerExceptionsPropertyByType = new();

	/// <summary>
	/// Combines multiple levels of messages into a single message.
	/// </summary>
	/// <param name="errorMetadata">The error metadata from which to get the messages.</param>
	/// <returns>The combined string.</returns>
	public static string CombineMessages(IErrorMetadata errorMetadata) =>
		GetMessage(errorMetadata, 0, 0);

	/// <summary>
	/// Combines multiple levels of stack traces into a single stack trace.
	/// </summary>
	/// <param name="errorMetadata">The error metadata from which to get the stack traces.</param>
	/// <returns>The combined string.</returns>
	public static string? CombineStackTraces(IErrorMetadata errorMetadata) =>
		GetStackTrace(errorMetadata, 0);

	/// <summary>
	/// Unwraps exceptions and their inner exceptions.
	/// </summary>
	/// <param name="ex">The exception to be converted.</param>
	/// <returns>The error metadata.</returns>
	public static (string?[] ExceptionTypes, string[] Messages, string?[] StackTraces, int[] ExceptionParentIndices, FailureCause Cause) ExtractMetadata(Exception ex)
	{
		Guard.ArgumentNotNull(ex);

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

		try
		{
			exceptionTypes.Add(ex.GetType().SafeName());
		}
		catch (Exception thrown)
		{
			exceptionTypes.Add(string.Format(CultureInfo.CurrentCulture, "<exception thrown while retrieving exception type: {0}>", thrown.Message));
		}

		try
		{
			messages.Add(ex.Message);
		}
		catch (Exception thrown)
		{
			messages.Add(string.Format(CultureInfo.CurrentCulture, "<exception thrown while retrieving exception message: {0}>", thrown.Message));
		}

		try
		{
			stackTraces.Add(ex.StackTrace);
		}
		catch (Exception thrown)
		{
			stackTraces.Add(string.Format(CultureInfo.CurrentCulture, "<exception thrown while retrieving exception stack trace: {0}>", thrown.Message));
		}

		indices.Add(parentIndex);

		try
		{
			var innerExceptions = GetInnerExceptions(ex);

			if (innerExceptions is not null)
				foreach (var innerException in innerExceptions)
					ExtractMetadata(innerException, myIndex, exceptionTypes, messages, stackTraces, indices);
			else if (ex.InnerException is not null && ex.StackTrace != ex.InnerException.StackTrace)
				ExtractMetadata(ex.InnerException, myIndex, exceptionTypes, messages, stackTraces, indices);
		}
		catch { }
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
		Guard.ArgumentNotNull(stackFrame);

#if DEBUG
		return false;
#else
		return stackFrame.StartsWith("at Xunit.", StringComparison.Ordinal);
#endif
	}

	static string? FilterStackTrace(string? stack)
	{
		if (stack is null)
			return null;

		var results = new List<string>();

		foreach (var line in stack.Split([Environment.NewLine], StringSplitOptions.None))
		{
			var trimmedLine = line.TrimStart();
			if (!FilterStackFrame(trimmedLine))
				results.Add(line);
		}

		return string.Join(Environment.NewLine, results.ToArray());
	}

	static string GetAt(
		string?[]? values,
		int index) =>
			values is not null && index >= 0 && values.Length > index
				? values[index] ?? string.Empty
				: string.Empty;

	static int GetAt(
		int[]? values,
		int index) =>
			values is not null && values.Length > index
				? values[index]
				: -1;

	static string GetMessage(
		IErrorMetadata errorMetadata,
		int index,
		int level)
	{
		Guard.ArgumentNotNull(errorMetadata);

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
		return nsIndex > 0 ? exceptionType.Substring(0, nsIndex) : "";
	}

	static string? GetStackTrace(
		IErrorMetadata errorMetadata,
		int index)
	{
		Guard.ArgumentNotNull(errorMetadata);

		var result = FilterStackTrace(GetAt(errorMetadata.StackTraces, index));

		var children = new List<int>();
		for (var subIndex = index + 1; subIndex < errorMetadata.ExceptionParentIndices.Length; ++subIndex)
			if (GetAt(errorMetadata.ExceptionParentIndices, subIndex) == index)
				children.Add(subIndex);

		if (children.Count > 1)
			for (var idx = 0; idx < children.Count; ++idx)
				result += string.Format(CultureInfo.CurrentCulture, "{0}----- Inner Stack Trace #{1} ({2}) -----{3}{4}", Environment.NewLine, idx + 1, GetAt(errorMetadata.ExceptionTypes, children[idx]), Environment.NewLine, GetStackTrace(errorMetadata, children[idx]));
		else if (children.Count == 1)
			result += string.Format(CultureInfo.CurrentCulture, "{0}----- Inner Stack Trace -----{1}{2}", Environment.NewLine, Environment.NewLine, GetStackTrace(errorMetadata, children[0]));

		return result;
	}
}
