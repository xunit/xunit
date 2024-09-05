namespace Xunit;

/// <summary>
/// Gives access to the current test context, which is considered to be an immutable snapshot of
/// the current test state at the time it's retrieved.
/// </summary>
public interface ITestContextAccessor
{
	/// <summary>
	/// Gets the current test context. If called outside of the text discovery or execution path,
	/// will return a test context that is in the <see cref="TestPipelineStage.Unknown"/> stage.
	/// The current test context is a "snapshot in time" for when this/ property is called, so do
	/// not cache the instance across a single method boundary (or else/ you run the risk of having
	/// an out-of-date context).
	/// </summary>
	ITestContext Current { get; }
}
