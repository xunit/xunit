namespace Xunit.Sdk;

/// <summary>
/// This is metadata describing the summary during various phases of execution process (e.g.,
/// test case, test class, test collection, and assembly). It describes the aggregation of
/// zero or more tests being executed.
/// </summary>
public interface IExecutionSummaryMetadata
{
	/// <summary>
	/// Gets the execution time (in seconds) for this execution.
	/// </summary>
	decimal ExecutionTime { get; }

	/// <summary>
	/// Gets the number of failing tests.
	/// </summary>
	int TestsFailed { get; }

	/// <summary>
	/// Gets the number of tests that were not run. This includes explicit tests when explicit tests are not run,
	/// or non-expicit tests when non-explicit tests are not run.
	/// </summary>
	int TestsNotRun { get; }

	/// <summary>
	/// Gets the number of skipped tests.
	/// </summary>
	int TestsSkipped { get; }

	/// <summary>
	/// Gets the total number of tests run and not run.
	/// </summary>
	int TestsTotal { get; }
}
