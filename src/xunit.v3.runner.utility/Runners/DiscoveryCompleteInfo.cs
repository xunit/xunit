namespace Xunit.Runners;

/// <summary>
/// Represents test discovery being completed.
/// </summary>
[Obsolete("Please use the DiscoveryCompleteInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
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
