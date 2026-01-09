namespace Xunit.Runner.Common;

/// <summary>
/// Collects execution totals for a group of test cases.
/// </summary>
public class ExecutionSummary
{
	/// <summary>
	/// Gets or set the total number of tests run.
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Gets or sets the number of failed tests.
	/// </summary>
	public int Failed { get; set; }

	/// <summary>
	/// Gets the number of passed tests.
	/// </summary>
	/// <remarks>
	/// This value is computed to be the total number of tests, minus the number of failed, skipped, and not run tests.
	/// </remarks>
	public virtual int Passed =>
		Total - Failed - Skipped - NotRun;

	/// <summary>
	/// Gets or sets the number of skipped tests.
	/// </summary>
	public int Skipped { get; set; }

	/// <summary>
	/// Gets or sets the number of tests not run.
	/// </summary>
	public int NotRun { get; set; }

	/// <summary>
	/// Gets or sets the total execution time for the tests.
	/// </summary>
	public decimal Time { get; set; }

	/// <summary>
	/// Gets or sets the total errors (i.e., cleanup failures) for the tests.
	/// </summary>
	public int Errors { get; set; }
}
