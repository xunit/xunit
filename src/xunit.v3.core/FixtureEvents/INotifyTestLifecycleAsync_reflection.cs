namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test lifecycle events (async).
/// </summary>
public interface INotifyTestLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test is finished.
	/// </summary>
	/// <param name="test">The test</param>
	ValueTask OnTestFinishedAsync(IXunitTest test);

	/// <summary>
	/// Called when the test is starting.
	/// </summary>
	/// <param name="test">The test</param>
	ValueTask OnTestStartingAsync(IXunitTest test);
}
