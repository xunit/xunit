using System;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Runner.v3;
using Xunit.Sdk;

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
	/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/>
	/// and <see cref="_InternalDiagnosticMessage"/> messages.</param>
	public static IFrontController? Create(
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

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		await innerController.DisposeAsync();
	}

	/// <inheritdoc/>
	public virtual int? Find(
		_IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerController.Find(messageSink, settings);
	}

	/// <inheritdoc/>
	public int? FindAndRun(
		_IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerController.FindAndRun(messageSink, settings);
	}

	/// <inheritdoc/>
	public int? Run(
		_IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		return innerController.Run(messageSink, settings);
	}
}
