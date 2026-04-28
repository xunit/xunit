namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test collection lifecycle events (non-async).
/// </summary>
/// <remarks>
/// Only assembly- and collection-level fixtures are eligible to be notified of this event.
/// </remarks>
public interface INotifyTestCollectionLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test collection is finished.
	/// </summary>
	/// <param name="testCollection">The test collection</param>
	void OnTestCollectionFinished(IXunitTestCollection testCollection);

	/// <summary>
	/// Called when the test collection is starting.
	/// </summary>
	/// <param name="testCollection">The test collection</param>
	void OnTestCollectionStarting(IXunitTestCollection testCollection);
}
