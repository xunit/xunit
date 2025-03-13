using System;

namespace Xunit.Sdk;

/// <summary>
/// This exception is throwing when a failure from <c>Trace.Assert</c> or
/// <c>Debug.Assert</c> has been detected.
/// </summary>
public class TraceAssertException : Exception
{
	const string header = "Trace/Debug.Assert() Failure";

	TraceAssertException(string? userMessage) :
		base(userMessage)
	{ }

	/// <summary>
	/// Creates a new instance of <see cref="TraceAssertException"/> to be thrown when a
	/// failure from <c>Trace.Assert</c> or <c>Debug.Assert</c> has been detected.
	/// </summary>
	/// <param name="message">The <c>message</c> value from the assert failure</param>
	/// <param name="detailMessage">The <c>detailMessage</c> value from the assert failure</param>
	/// <returns></returns>
	public static TraceAssertException ForAssertFailure(
		string? message,
		string? detailMessage) =>
			new((message, detailMessage) switch
			{
				(null or "", null or "") => header,
				(_, null or "") => $"{header}: {message}",
				(null or "", _) => $"{header}: {detailMessage}",
				_ => $"{header}: {message}{Environment.NewLine}{detailMessage}",
			});
}
