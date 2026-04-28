namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test case lifecycle events (non-async).
/// </summary>
public interface INotifyTestCaseLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test case is finished.
	/// </summary>
	/// <param name="testCase">The test case</param>
	void OnTestCaseFinished(IXunitTestCase testCase);

	/// <summary>
	/// Called when the test case is starting.
	/// </summary>
	/// <param name="testCase">The test case</param>
	void OnTestCaseStarting(IXunitTestCase testCase);
}
