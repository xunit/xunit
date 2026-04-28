namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test method lifecycle events (async).
/// </summary>
public interface INotifyTestMethodLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test method is finished.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	ValueTask OnTestMethodFinishedAsync(ICodeGenTestMethod testMethod);

	/// <summary>
	/// Called when the test method is starting.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	ValueTask OnTestMethodStartingAsync(ICodeGenTestMethod testMethod);
}
