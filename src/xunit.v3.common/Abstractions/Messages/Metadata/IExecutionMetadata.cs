namespace Xunit.Sdk;

/// <summary>
/// This is metadata describing the execution of a single test.
/// </summary>
public interface IExecutionMetadata
{
	/// <summary>
	/// The time spent executing the test, in seconds. Will be 0 if the test was not executed.
	/// </summary>
	decimal ExecutionTime { get; }

	/// <summary>
	/// The captured output of the test. Will be <see cref="string.Empty"/> if there was no output.
	/// </summary>
	string Output { get; }

	/// <summary>
	/// Gets a list of the warning messages that were recorded during execution. Will be <c>null</c>
	/// if there were no warnings.
	/// </summary>
	string[]? Warnings { get; }
}
