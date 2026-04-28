namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test method lifecycle events (non-async).
/// </summary>
public interface INotifyTestMethodLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test method is finished.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	void OnTestMethodFinished(IXunitTestMethod testMethod);

	/// <summary>
	/// Called when the test method is starting.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	void OnTestMethodStarting(IXunitTestMethod testMethod);
}
