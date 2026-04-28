namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test collection lifecycle events (async).
/// </summary>
/// <remarks>
/// Only assembly- and collection-level fixtures are eligible to be notified of this event.
/// </remarks>
public interface INotifyTestCollectionLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test collection is finished.
	/// </summary>
	/// <param name="testCollection">The test collection</param>
	ValueTask OnTestCollectionFinishedAsync(IXunitTestCollection testCollection);

	/// <summary>
	/// Called when the test collection is starting.
	/// </summary>
	/// <param name="testCollection">The test collection</param>
	ValueTask OnTestCollectionStartingAsync(IXunitTestCollection testCollection);
}
