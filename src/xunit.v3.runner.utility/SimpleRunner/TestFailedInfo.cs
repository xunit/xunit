using Xunit.Sdk;

namespace Xunit.SimpleRunner;

/// <summary>
/// Represents a test that failed.
/// </summary>
public class TestFailedInfo : TestFinishedInfo
{
	/// <summary>
	/// Gets the cause of the test failure.
	/// </summary>
	/// <remarks>
	/// For v1 or v2 test projects, this value will always be <see cref="FailureCause.Exception"/>.<br />
	/// For v3 test projects, all values of <see cref="FailureCause"/> are possible.
	/// </remarks>
	public required FailureCause Cause { get; set; }

	/// <summary>
	/// Gets the exception that caused the test to fail.
	/// </summary>
	public required ExceptionInfo Exception { get; set; }
}
