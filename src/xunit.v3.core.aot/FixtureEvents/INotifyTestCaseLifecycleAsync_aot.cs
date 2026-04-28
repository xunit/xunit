namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test case lifecycle events (async).
/// </summary>
public interface INotifyTestCaseLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test case is finished.
	/// </summary>
	/// <param name="testCase">The test case</param>
	ValueTask OnTestCaseFinishedAsync(ICodeGenTestCase testCase);

	/// <summary>
	/// Called when the test case is starting.
	/// </summary>
	/// <param name="testCase">The test case</param>
	ValueTask OnTestCaseStartingAsync(ICodeGenTestCase testCase);
}
