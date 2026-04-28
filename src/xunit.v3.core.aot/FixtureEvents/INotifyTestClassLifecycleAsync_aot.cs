namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test class lifecycle events (async).
/// </summary>
public interface INotifyTestClassLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test class is finished.
	/// </summary>
	/// <param name="testClass">The test class</param>
	ValueTask OnTestClassFinishedAsync(ICodeGenTestClass testClass);

	/// <summary>
	/// Called when the test class is starting.
	/// </summary>
	/// <param name="testClass">The test class</param>
	ValueTask OnTestClassStartingAsync(ICodeGenTestClass testClass);
}
