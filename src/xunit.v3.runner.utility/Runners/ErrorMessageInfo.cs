namespace Xunit.Runners;

/// <summary>
/// Represents an error that happened outside the scope of a running test.
/// </summary>
/// <summary/>
public class ErrorMessageInfo(
	ErrorMessageType messageType,
	string? exceptionType,
	string exceptionMessage,
	string? exceptionStackTrace)
{
	/// <summary>
	/// The type of error condition that was encountered.
	/// </summary>
	public ErrorMessageType MesssageType { get; } = messageType;

	/// <summary>
	/// The exception that caused the test failure.
	/// </summary>
	public string? ExceptionType { get; } = exceptionType;

	/// <summary>
	/// The message from the exception that caused the test failure.
	/// </summary>
	public string ExceptionMessage { get; } = exceptionMessage;

	/// <summary>
	/// The stack trace from the exception that caused the test failure.
	/// </summary>
	public string? ExceptionStackTrace { get; } = exceptionStackTrace;
}
