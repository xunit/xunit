namespace Xunit;

/// <summary>
/// Indicates the result of running the test.
/// </summary>
public enum TestResult
{
	/// <summary>
	/// The test passed.
	/// </summary>
	Passed,

	/// <summary>
	/// The test failed.
	/// </summary>
	Failed,

	/// <summary>
	/// The test was skipped.
	/// </summary>
	Skipped,

	/// <summary>
	/// The test was not run because it was excluded (either because it was marked as explicit
	/// and explicit tests weren't run, or because it was marked as not explicit as only explicit
	/// tests were run).
	/// </summary>
	NotRun,
}
