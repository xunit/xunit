using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.SimpleRunner;

/// <summary>
/// Contains information about an exception, as well as any nested exceptions.
/// </summary>
public class ExceptionInfo
{
	/// <summary>
	/// Gets the fully qualified type name of the exception. If the type is unknown
	/// or not a CLR type, this may return <c>null</c>.
	/// </summary>
	public required string? FullType { get; set; }

	/// <summary>
	/// Gets the list of inner exceptions for this exception.
	/// </summary>
	public List<ExceptionInfo> InnerExceptions { get; } = [];

	/// <summary>
	/// Gets the exception message.
	/// </summary>
	public required string Message { get; set; }

	/// <summary>
	/// Gets the exception stack trace.
	/// </summary>
	public required string? StackTrace { get; set; }

	internal static ExceptionInfo FromErrorMessage(IErrorMetadata errorMessage)
	{
		var results = new List<ExceptionInfo>();
		var length = Math.Min(errorMessage.ExceptionParentIndices.Length, errorMessage.ExceptionTypes.Length);
		length = Math.Min(length, errorMessage.Messages.Length);
		length = Math.Min(length, errorMessage.StackTraces.Length);

		for (var idx = 0; idx < length; ++idx)
			results.Add(new ExceptionInfo
			{
				FullType = errorMessage.ExceptionTypes[idx],
				Message = errorMessage.Messages[idx],
				StackTrace = errorMessage.StackTraces[idx]
			});

		for (var idx = 0; idx < length; ++idx)
		{
			var parentIdx = errorMessage.ExceptionParentIndices[idx];
			if (parentIdx >= 0)
				results[parentIdx].InnerExceptions.Add(results[idx]);
		}

		return results[0];
	}
}
