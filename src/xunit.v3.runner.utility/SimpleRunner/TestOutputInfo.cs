namespace Xunit.SimpleRunner;

/// <summary>
/// Represents a single line of live test output.
/// </summary>
public class TestOutputInfo : TestInfo
{
	/// <summary>
	/// Gets a single line of live output.
	/// </summary>
	public required string Output { get; set; }
}
