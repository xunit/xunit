using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implement this to control the launch of an xUnit.net v3 test process.
/// </summary>
/// <remarks>
/// This is a higher level API than <see cref="ITestProcessLauncher"/> which allows more optimized
/// communication rather than mandating all message passing is done via text readers/writers that
/// represent standard input and standard output. Launchers that do not implement this interface
/// with be adapted by <see cref="TestProcessLauncherAdapter"/>.
/// </remarks>
public interface ITestProcessDirectLauncher
{
	/// <summary>
	/// Gets the test assembly metadata.
	/// </summary>
	/// <param name="projectAssembly">The test assembly</param>
	TestAssemblyInfo GetAssemblyInfo(XunitProjectAssembly projectAssembly);

	/// <summary>
	/// Starts the process of finding tests in an assembly.
	/// </summary>
	/// <param name="projectAssembly">The test assembly</param>
	/// <param name="assemblyInfo">The test assembly information (from <see cref="GetAssemblyInfo"/>)</param>
	/// <param name="messageSink">The message sink to report results back to.</param>
	/// <param name="diagnosticMessageSink">The message to report diagnostic messages to.</param>
	/// <param name="sourceInformationProvider">The source information provider used to add file and line
	/// information to discovered tests</param>
	ITestProcessBase Find(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider);

	/// <summary>
	/// Starts the process of running tests in the assembly.
	/// </summary>
	/// <param name="projectAssembly">The test assembly</param>
	/// <param name="assemblyInfo">The test assembly information (from <see cref="GetAssemblyInfo"/>)</param>
	/// <param name="messageSink">The message sink to report results back to.</param>
	/// <param name="diagnosticMessageSink">The message to report diagnostic messages to.</param>
	/// <param name="sourceInformationProvider">The source information provider used to add file and line
	/// information to discovered tests</param>
	ITestProcessBase Run(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider
	);
}
