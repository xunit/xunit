using System;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Runner.v3;
using Xunit.Sdk;
using Xunit.v3;

#if NETFRAMEWORK
using Xunit.Runner.v1;
#endif

namespace Xunit;

#if NETFRAMEWORK
/// <summary>
/// Default implementation of <see cref="IFrontController"/> which supports running tests from
/// xUnit.net v1, v2, and v3.
/// </summary>
#else
/// <summary>
/// Default implementation of <see cref="IFrontController"/> which supports running tests from
/// xUnit.net v2 and v3.
/// </summary>
#endif
public class XunitFrontController : IFrontController
{
	bool disposed;
	readonly IFrontController innerController;

	XunitFrontController(IFrontController innerController) =>
		this.innerController = Guard.ArgumentNotNull(innerController);

	/// <inheritdoc/>
	public bool CanUseAppDomains => innerController.CanUseAppDomains;

	/// <inheritdoc/>
	public string TargetFramework => innerController.TargetFramework;

	/// <inheritdoc/>
	public string TestAssemblyUniqueID => innerController.TestAssemblyUniqueID;

	/// <inheritdoc/>
	public string TestFrameworkDisplayName => innerController.TestFrameworkDisplayName;

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used for both discovery and execution.
	/// If the assembly does not appear to be a test assembly, returns <c>null</c>.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="testProcessLauncher">The test process launcher, used to launch v3 test processes.
	/// If not provided, <see cref="LocalOutOfProcessTestProcessLauncher"/> will be used. (This value is
	/// not used when running v1 or v2 test proceses.)</param>
	public static IFrontController? Create(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider = null,
		IMessageSink? diagnosticMessageSink = null,
		ITestProcessLauncher? testProcessLauncher = null)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(projectAssembly.AssemblyMetadata);

		var assemblyFileName = projectAssembly.AssemblyFileName;
		var assemblyFolder = Path.GetDirectoryName(assemblyFileName);

		return projectAssembly.AssemblyMetadata.XunitVersion switch
		{
			3 => new XunitFrontController(Xunit3.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink, testProcessLauncher)),
			2 => new XunitFrontController(Xunit2.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#if NETFRAMEWORK
			1 => new XunitFrontController(Xunit1.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#endif
			_ => null,
		};
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		await innerController.SafeDisposeAsync();
	}

	/// <inheritdoc/>
	public virtual void Find(
		IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		innerController.Find(messageSink, settings);
	}

	/// <inheritdoc/>
	public void FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		innerController.FindAndRun(messageSink, settings);
	}

	/// <inheritdoc/>
	public void Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		innerController.Run(messageSink, settings);
	}
}
