namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test class lifecycle events (non-async).
/// </summary>
public interface INotifyTestClassLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test class is finished.
	/// </summary>
	/// <param name="testClass">The test class</param>
	void OnTestClassFinished(ICodeGenTestClass testClass);

	/// <summary>
	/// Called when the test class is starting.
	/// </summary>
	/// <param name="testClass">The test class</param>
	void OnTestClassStarting(ICodeGenTestClass testClass);
}
