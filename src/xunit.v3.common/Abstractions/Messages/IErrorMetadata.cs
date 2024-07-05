namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about an error during test discovery or execution.
/// </summary>
public interface IErrorMetadata
{
	/// <summary>
	/// Gets the parent exception index(es) for the exception(s); a -1 indicates
	/// that the exception in question has no parent.
	/// </summary>
	int[] ExceptionParentIndices { get; }

	/// <summary>
	/// Gets the fully-qualified type name(s) of the exception(s).
	/// </summary>
	string?[] ExceptionTypes { get; }

	/// <summary>
	/// Gets the message(s) of the exception(s).
	/// </summary>
	string[] Messages { get; }

	/// <summary>
	/// Gets the stack trace(s) of the exception(s).
	/// </summary>
	string?[] StackTraces { get; }
}
