namespace Xunit.SimpleRunner;

/// <summary>
/// Indicates that test execution has completed.
/// </summary>
public class ExecutionCompleteInfo : ExecutionStartingInfo
{
	/// <summary>
	/// Gets the execution time (in seconds) for this execution.
	/// </summary>
	public required decimal ExecutionTime { get; set; }

	/// <summary>
	/// Gets the date and time when the test assembly execution finished.
	/// </summary>
	public required DateTimeOffset FinishTime { get; set; }

	/// <summary>
	/// Gets the number of errors that occurred.
	/// </summary>
	/// <remarks>
	/// This is a count of the number of <see cref="ErrorMessageInfo"/> that were reported.
	/// </remarks>
	public required int TotalErrors { get; set; }

	/// <summary>
	/// Gets the number of tests which failed.
	/// </summary>
	/// <remarks>
	/// This is a count of the number of <see cref="TestSkippedInfo"/> that were reported.
	/// </remarks>
	public required int TestsFailed { get; set; }

	/// <summary>
	/// Gets the number of tests that were not run. This includes explicit tests when explicit tests are not run,
	/// or non-expicit tests when non-explicit tests are not run.
	/// </summary>
	/// <remarks>
	/// This is a count of the number of <see cref="TestNotRunInfo"/> that were reported.
	/// </remarks>
	public required int TestsNotRun { get; set; }

	/// <summary>
	/// Gets the number of tests which passed.
	/// </summary>
	/// <remarks>
	/// This is a count of the number of <see cref="TestPassedInfo"/> that were reported.
	/// </remarks>
	public int TestsPassed => TestsTotal - TestsFailed - TestsNotRun - TestsSkipped;

	/// <summary>
	/// Gets the number of skipped tests.
	/// </summary>
	/// <remarks>
	/// This is a count of the number of <see cref="TestSkippedInfo"/> that were reported.
	/// </remarks>
	public required int TestsSkipped { get; set; }

	/// <summary>
	/// Gets the total number of tests.
	/// </summary>
	/// <remarks>
	/// This is the sum total of <see cref="TestsFailed"/>, <see cref="TestsNotRun"/>, <see cref="TestsPassed"/>,
	/// and <see cref="TestsSkipped"/>.
	/// </remarks>
	public required int TestsTotal { get; set; }
}
