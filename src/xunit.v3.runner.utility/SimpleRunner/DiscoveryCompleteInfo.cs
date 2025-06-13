namespace Xunit.SimpleRunner;

/// <summary>
/// Represents test discovery being completed.
/// </summary>
public class DiscoveryCompleteInfo
{
	/// <summary>
	/// Gets the number of test cases that will be run, after filtering was applied.
	/// </summary>
	public required int TestCasesToRun { get; set; }
}
