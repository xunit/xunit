using System;

namespace Xunit.SimpleRunner;

/// <summary>
/// Represents a test that is starting.
/// </summary>
public class TestStartingInfo : TestInfo
{
	/// <summary>
	/// Gets a flag which indicates whether the test was marked as explicit.
	/// </summary>
	public required bool Explicit { get; set; }

	/// <summary>
	/// Gets the date and time when the test execution started.
	/// </summary>
	public required DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// Gets the timeout for the test, in milliseconds; if <c>0</c>, there is no timeout.
	/// </summary>
	public required int Timeout { get; set; }
}
