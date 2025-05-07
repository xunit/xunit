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
	int? ExitCode { get; }
}
