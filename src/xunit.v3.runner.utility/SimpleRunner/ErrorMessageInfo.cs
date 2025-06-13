namespace Xunit.SimpleRunner;

/// <summary>
/// Represents an error that happened outside the scope of a running test.
/// </summary>
public class ErrorMessageInfo
{
	/// <summary>
	/// Gets the type of the error message.
	/// </summary>
	public required ErrorMessageType ErrorMessageType { get; set; }

	/// <summary>
	/// Gets the exception that caused the error.
	/// </summary>
	public required ExceptionInfo Exception { get; set; }
}
