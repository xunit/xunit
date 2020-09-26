#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This is metadata describing the execution of a single test.
	/// </summary>
	public interface _IExecutionMetadata
	{
		/// <summary>
		///The time spent executing the test, in seconds. May be <c>null</c>
		/// if the test was not executed.
		/// </summary>
		decimal? ExecutionTime { get; }

		/// <summary>
		/// The captured output of the test.
		/// </summary>
		string? Output { get; }
	}
}
