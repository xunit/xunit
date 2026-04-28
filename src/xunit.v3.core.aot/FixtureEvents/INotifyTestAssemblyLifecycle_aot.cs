namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test assembly lifecycle events (non-async).
/// </summary>
/// <remarks>
/// Only assembly-level fixtures are eligible to be notified of this event.
/// </remarks>
public interface INotifyTestAssemblyLifecycle : INotifyLifecycle
{
	/// <summary>
	/// Called when the test assembly is finished.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	void OnTestAssemblyFinished(ICodeGenTestAssembly testAssembly);

	/// <summary>
	/// Called when the test assembly is starting.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	void OnTestAssemblyStarting(ICodeGenTestAssembly testAssembly);
}
