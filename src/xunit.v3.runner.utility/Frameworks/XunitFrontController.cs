using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Runner.v3;
using Xunit.v3;

#if NETFRAMEWORK
using Xunit.Runner.v1;
#endif

namespace Xunit;

/// <summary>
/// Default implementation of <see cref="IFrontController"/> which supports running tests from
/// all of xUnit.net v1, v2, and v3.
/// </summary>
public class XunitFrontController : IFrontController
{
	bool disposed;
	readonly IFrontController? innerController;
	readonly IFrontControllerDiscoverer innerDiscoverer;

	// Discovery constructor
	XunitFrontController(IFrontControllerDiscoverer innerDiscoverer)
	{
		this.innerDiscoverer = Guard.ArgumentNotNull(innerDiscoverer);
	}

	// Discovery + execution constructor
	XunitFrontController(IFrontController innerController) :
		this((IFrontControllerDiscoverer)innerController)
	{
		this.innerController = Guard.ArgumentNotNull(innerController);
	}

	/// <inheritdoc/>
	public bool CanUseAppDomains => innerDiscoverer.CanUseAppDomains;

	/// <inheritdoc/>
	public string TargetFramework => innerDiscoverer.TargetFramework;

	/// <inheritdoc/>
	public string TestAssemblyUniqueID => innerDiscoverer.TestAssemblyUniqueID;

	/// <inheritdoc/>
	public string TestFrameworkDisplayName => innerDiscoverer.TestFrameworkDisplayName;

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		await innerDiscoverer.DisposeAsync();

		if (innerController is not null)
			await innerController.DisposeAsync();
	}

	/// <inheritdoc/>
	public virtual int? Find(
		_IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerDiscoverer.Find(messageSink, settings);
	}

	/// <inheritdoc/>
	public int? FindAndRun(
		_IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "This instance of {0} was created for discovery only; execution-related operations cannot be performed.", typeof(XunitFrontController).FullName), innerController);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerController.FindAndRun(messageSink, settings);
	}

	/// <inheritdoc/>
	public int? Run(
		_IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "This instance of {0} was created for discovery only; execution-related operations cannot be performed.", typeof(XunitFrontController).FullName), innerController);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerController.Run(messageSink, settings);
	}

	// Factory methods

	/// <summary>
	/// Returns an implementation of <see cref="IFrontControllerDiscoverer"/> which can be
	/// used to discover tests (including source-based discovery for v2 tests).
	/// If the assembly does not appear to be a test assembly, returns <c>null</c>.
	/// </summary>
	/// <param name="assemblyInfo">The assembly to use for discovery</param>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
	public static IFrontControllerDiscoverer? ForDiscovery(
		_IAssemblyInfo assemblyInfo,
		XunitProjectAssembly projectAssembly,
		_ISourceInformationProvider? sourceInformationProvider = null,
		_IMessageSink? diagnosticMessageSink = null)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(projectAssembly.AssemblyMetadata);

		return projectAssembly.AssemblyMetadata.XunitVersion switch
		{
			3 => new XunitFrontController(Xunit3.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
			2 => new XunitFrontController(Xunit2.ForDiscovery(assemblyInfo, projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#if NETFRAMEWORK
			1 => new XunitFrontController(Xunit1.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#endif
			_ => null,
		};
	}

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used for both discovery and execution.
	/// If the assembly does not appear to be a test assembly, returns <c>null</c>.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/>
	/// and <see cref="_InternalDiagnosticMessage"/> messages.</param>
	public static IFrontController? ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		_ISourceInformationProvider? sourceInformationProvider = null,
		_IMessageSink? diagnosticMessageSink = null)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(projectAssembly.AssemblyMetadata);

		var assemblyFileName = projectAssembly.AssemblyFileName;
		var assemblyFolder = Path.GetDirectoryName(assemblyFileName);

		return projectAssembly.AssemblyMetadata.XunitVersion switch
		{
			3 => new XunitFrontController(Xunit3.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
			2 => new XunitFrontController(Xunit2.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#if NETFRAMEWORK
			1 => new XunitFrontController(Xunit1.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink)),
#endif
			_ => null,
		};
	}
}
