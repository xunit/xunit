namespace Xunit.v3
{
	/// <summary>
	/// This is metadata describing the execution of a single test.
	/// </summary>
	public interface _IExecutionMetadata
	{
		/// <summary>
		/// The time spent executing the test, in seconds. Will be 0 if the test was not executed.
		/// </summary>
		decimal ExecutionTime { get; }

		/// <summary>
		/// The captured output of the test. Will be <see cref="string.Empty"/> if there was no output.
		/// </summary>
		string Output { get; }
	}
}
