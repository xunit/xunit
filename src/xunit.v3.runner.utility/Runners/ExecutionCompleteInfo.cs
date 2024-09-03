namespace Xunit.Runners;

/// <summary>
/// Represents test assembly execution being finished.
/// </summary>
public class ExecutionCompleteInfo(
	int totalTests,
	int testsFailed,
	int testsSkipped,
	int testsNotRun,
	decimal executionTime)
{
	/// <summary>
	/// The total execution time spent running tests.
	/// </summary>
	public decimal ExecutionTime { get; } = executionTime;

	/// <summary>
	/// The number of the tests that failed.
	/// </summary>
	public int TestsFailed { get; } = testsFailed;

	/// <summary>
	/// The number of tests that were not run.
	/// </summary>
	public int TestsNotRun { get; } = testsNotRun;

	/// <summary>
	/// The number of tests that were skipped.
	/// </summary>
	public int TestsSkipped { get; } = testsSkipped;

	/// <summary>
	/// The total number of tests in the assembly.
	/// </summary>
	public int TotalTests { get; } = totalTests;

	/// <summary>
	/// Used to report results when no tests are executed.
	/// </summary>
	public static readonly ExecutionCompleteInfo Empty = new(0, 0, 0, 0, 0M);
}
