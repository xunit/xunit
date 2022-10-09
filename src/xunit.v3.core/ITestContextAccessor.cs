namespace Xunit;

/// <summary>
/// Gives access to the current test context, which is considered to be an immutable snapshot of
/// the current test state at the time it's retrieved.
/// </summary>
public interface ITestContextAccessor
{
	/// <summary>
	/// Gets a snapshot of the current state of test execution.
	/// </summary>
	TestContext? Current { get; }
}
