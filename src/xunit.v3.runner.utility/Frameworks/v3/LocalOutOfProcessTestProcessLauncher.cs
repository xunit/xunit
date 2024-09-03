namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcessLauncher"/> that will launch an xUnit.net v3 test
/// project out-of-process on the local machine.
/// </summary>
public sealed class LocalOutOfProcessTestProcessLauncher : OutOfProcessTestProcessLauncherBase
{
	LocalOutOfProcessTestProcessLauncher()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="LocalOutOfProcessTestProcessLauncher"/>.
	/// </summary>
	public static LocalOutOfProcessTestProcessLauncher Instance { get; } = new();

	/// <inheritdoc/>
	protected sealed override ITestProcess? StartTestProcess(
		string executable,
		string executableArguments,
		string? responseFile) =>
			LocalTestProcess.Start(executable, executableArguments, responseFile);
}
