namespace Xunit.Runners;

/// <summary>
/// Represents test discovery being completed.
/// </summary>
public class DiscoveryCompleteInfo(
	int testCasesDiscovered,
	int testCasesToRun)
{
	/// <summary>
	/// The number of test cases that were discovered.
	/// </summary>
	public int TestCasesDiscovered { get; } = testCasesDiscovered;

	/// <summary>
	/// The number of test cases that will be run, after filtering was applied.
	/// </summary>
	public int TestCasesToRun { get; } = testCasesToRun;
}
