namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test lifecycle events (non-async).
/// </summary>
public interface INotifyTestLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test is finished.
	/// </summary>
	/// <param name="test">The test</param>
	void OnTestFinished(IXunitTest test);

	/// <summary>
	/// Called when the test is starting.
	/// </summary>
	/// <param name="test">The test</param>
	void OnTestStarting(IXunitTest test);
}
