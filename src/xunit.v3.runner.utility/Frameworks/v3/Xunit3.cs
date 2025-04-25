using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// This class is used to do discovery and execution of xUnit.net v3 tests.
/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
/// instead of using this class directly.
/// </summary>
public class Xunit3 : IFrontController
{
	readonly IMessageSink? diagnosticMessageSink;
	readonly DisposalTracker disposalTracker = new();
	readonly XunitProjectAssembly projectAssembly;
#pragma warning disable CA2213  // This is disposed by DisposalTracker when appropriate
	readonly ISourceInformationProvider sourceInformationProvider;
#pragma warning restore CA2213
	readonly TestAssemblyInfo testAssemblyInfo;
	readonly ITestProcessDirectLauncher testProcessLauncher;

	Xunit3(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider,
		IMessageSink? diagnosticMessageSink,
		ITestProcessLauncher testProcessLauncher)
	{
		if (sourceInformationProvider is null)
		{
			sourceInformationProvider = CecilSourceInformationProvider.Create(projectAssembly.AssemblyFileName);
			disposalTracker.Add(sourceInformationProvider);
		}

		this.projectAssembly = projectAssembly;
		this.sourceInformationProvider = sourceInformationProvider;
		this.diagnosticMessageSink = diagnosticMessageSink;
		this.testProcessLauncher = AdaptLauncher(testProcessLauncher);

		testAssemblyInfo = this.testProcessLauncher.GetAssemblyInfo(projectAssembly);
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
	}

	/// <inheritdoc/>
	public bool CanUseAppDomains =>
		false;

	/// <summary>
	/// Gets the version of <c>xunit.v3.core.dll</c> the test assembly is linked against.
	/// </summary>
	public Version CoreFrameworkVersion =>
		testAssemblyInfo.CoreFramework;

	/// <summary>
	/// Gets the informational version of <c>xunit.v3.core.dll</c> the test assembly
	/// is linked against.
	/// </summary>
	public string CoreFrameworkVersionInformational =>
		testAssemblyInfo.CoreFrameworkInformational;

	/// <inheritdoc/>
	public string TargetFramework =>
		testAssemblyInfo.TargetFramework;

	/// <inheritdoc/>
	public string TestAssemblyUniqueID { get; }

	/// <inheritdoc/>
	public string TestFrameworkDisplayName => testAssemblyInfo.TestFramework;

	static ITestProcessDirectLauncher AdaptLauncher(ITestProcessLauncher launcher) =>
		launcher as ITestProcessDirectLauncher ?? new TestProcessLauncherAdapter(launcher);

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		await disposalTracker.SafeDisposeAsync();
	}

	/// <inheritdoc/>
	public void Find(
		IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var projectAssembly = this.projectAssembly.WithSettings(settings);
		var process = testProcessLauncher.Find(projectAssembly, testAssemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);

		try
		{
			process.WaitForExit(int.MaxValue);
		}
		finally
		{
			process.SafeDispose();
		}
	}

	/// <inheritdoc/>
	public void FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var projectAssembly = this.projectAssembly.WithSettings(settings);
		var process = testProcessLauncher.Run(projectAssembly, testAssemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);

		try
		{
			process.WaitForExit(int.MaxValue);
		}
		finally
		{
			process.SafeDispose();
		}
	}

	/// <inheritdoc/>
	public void Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);
		Guard.ArgumentNotNullOrEmpty(settings.SerializedTestCases);

		var projectAssembly = this.projectAssembly.WithSettings(settings);
		var process = testProcessLauncher.Run(projectAssembly, testAssemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);

		try
		{
			process.WaitForExit(int.MaxValue);
		}
		finally
		{
			process.SafeDispose();
		}
	}

	// Factory method

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used
	/// for both discovery and execution of xUnit.net v3 tests.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="testProcessLauncher">The test process launcher to use for starting the xUnit.net v3 test
	/// process. If not specified, will use <see cref="LocalOutOfProcessTestProcessLauncher"/>.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider = null,
		IMessageSink? diagnosticMessageSink = null,
		ITestProcessLauncher? testProcessLauncher = null)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.FileExists(projectAssembly.AssemblyFileName);

		return new Xunit3(
			projectAssembly,
			sourceInformationProvider,
			diagnosticMessageSink,
			testProcessLauncher ?? LocalOutOfProcessTestProcessLauncher.Instance
		);
	}
}
