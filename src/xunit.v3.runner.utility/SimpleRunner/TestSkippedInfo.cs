namespace Xunit.SimpleRunner;

/// <summary>
/// Represents a test that was skipped.
/// </summary>
public class TestSkippedInfo : TestFinishedInfo
{
	/// <summary>
	/// Gets the reason that was given for skipping the test.
	/// </summary>
	public required string SkipReason { get; set; }
}
