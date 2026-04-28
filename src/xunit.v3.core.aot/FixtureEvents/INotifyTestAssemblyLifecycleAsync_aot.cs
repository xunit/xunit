namespace Xunit.v3;

/// <summary>
/// Allows a fixture to be notified about test assembly lifecycle events (async).
/// </summary>
/// <remarks>
/// Only assembly-level fixtures are eligible to be notified of this event.
/// </remarks>
public interface INotifyTestAssemblyLifecycleAsync : INotifyLifecycle
{
	/// <summary>
	/// Called when the test assembly is finished.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	ValueTask OnTestAssemblyFinishedAsync(ICodeGenTestAssembly testAssembly);

	/// <summary>
	/// Called when the test assembly is starting.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	ValueTask OnTestAssemblyStartingAsync(ICodeGenTestAssembly testAssembly);
}
