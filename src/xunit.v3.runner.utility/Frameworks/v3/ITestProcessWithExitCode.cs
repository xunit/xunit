namespace Xunit.v3;

/// <summary>
/// Marks a test process which has an exit code after it has completed.
/// </summary>
public interface ITestProcessWithExitCode
{
	/// <summary>
	/// Gets the exit code that the process exited with. If the process has not yet exited,
	/// or the exit code was unknown, will return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// Since disposing of <see cref="ITestProcess"/> ensures for the orderly shutdown of the
	/// process (for example, ensuring that all the standard output has been consumed after the
	/// process has exited), it should remain legal to retrieve the exit code even after the
	/// original process object has been disposed.
	/// </remarks>
	int? ExitCode { get; }
}
