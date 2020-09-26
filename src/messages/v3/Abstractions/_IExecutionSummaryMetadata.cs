#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This is metadata describing the summary during various phases of execution process (e.g.,
	/// test case, test class, test collection, and assembly). It describes the aggregation of
	/// zero or more tests being executed.
	/// </summary>
	public interface _IExecutionSummaryMetadata
	{
		/// <summary>
		/// The execution time (in seconds) for this execution.
		/// </summary>
		decimal ExecutionTime { get; }

		/// <summary>
		/// The number of failing tests.
		/// </summary>
		int TestsFailed { get; }

		/// <summary>
		/// The total number of tests run.
		/// </summary>
		int TestsRun { get; }

		/// <summary>
		/// The number of skipped tests.
		/// </summary>
		int TestsSkipped { get; }
	}
}
