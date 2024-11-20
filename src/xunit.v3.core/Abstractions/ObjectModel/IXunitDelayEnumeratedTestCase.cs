namespace Xunit.v3;

/// <summary>
/// Indicates a test case which delays enumeration of tests until execution time.
/// </summary>
/// <remarks>
/// This is normally used when theory data enumeration is performed at execution time, either because theory
/// data pre-enumeration was disabled, or data found during discovery was not serializable.
/// </remarks>
public interface IXunitDelayEnumeratedTestCase : IXunitTestCase
{
	/// <summary>
	/// Get a flag to indicate whether test cases with no data should be skipped (instead
	/// of failed, which is the default behavior).
	/// </summary>
	bool SkipTestWithoutData { get; }
}
