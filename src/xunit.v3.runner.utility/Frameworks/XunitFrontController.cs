using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.v3;

#if NETFRAMEWORK
using Xunit.Runner.v1;
#endif

namespace Xunit
{
	/// <summary>
	/// Default implementation of <see cref="IFrontController"/> which supports running tests from
	/// both xUnit.net v1 and v2.
	/// </summary>
	public class XunitFrontController : IFrontController
	{
		bool disposed;
		IFrontController? innerController;
		IFrontControllerDiscoverer innerDiscoverer;

		// Discovery controller
		XunitFrontController(IFrontControllerDiscoverer innerDiscoverer)
		{
			this.innerDiscoverer = Guard.ArgumentNotNull(nameof(innerDiscoverer), innerDiscoverer);
		}

		// Discovery + execution constructor
		XunitFrontController(IFrontController innerController) :
			this((IFrontControllerDiscoverer)innerController)
		{
			this.innerController = Guard.ArgumentNotNull(nameof(innerController), innerController);
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
		public ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			return innerDiscoverer.DisposeAsync();
		}

		/// <inheritdoc/>
		public virtual void Find(
			_IMessageSink messageSink,
			FrontControllerFindSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			innerDiscoverer.Find(messageSink, settings);
		}

		/// <inheritdoc/>
		public void FindAndRun(
			_IMessageSink messageSink,
			FrontControllerFindAndRunSettings settings)
		{
			Guard.NotNull($"This instance of {typeof(XunitFrontController).FullName} is for discovery only", innerController);

			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			innerController.FindAndRun(messageSink, settings);
		}

		/// <inheritdoc/>
		public void Run(
			_IMessageSink messageSink,
			FrontControllerRunSettings settings)
		{
			Guard.NotNull($"This instance of {typeof(XunitFrontController).FullName} is for discovery only", innerController);

			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			innerController.Run(messageSink, settings);
		}

		// Factory methods

		/// <summary>
		/// Returns an implementation of <see cref="IFrontControllerDiscoverer"/> which can be
		/// used to discovery tests, including source-based discovery (note that xUnit.net v1
		/// does not support source-based discovery).
		/// </summary>
		/// <param name="assemblyInfo">The assembly to use for discovery</param>
		/// <param name="projectAssembly">The test project assembly.</param>
		/// <param name="referenceList">The full path names of all referenced assemblies. This is used to
		/// search for references to specific xUnit.net reference assemblies to determine which version
		/// of xUnit.net the tests were written against.</param>
		/// <param name="sourceInformationProvider">The optional source information provider.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <returns></returns>
		public static IFrontControllerDiscoverer ForDiscovery(
			_IAssemblyInfo assemblyInfo,
			XunitProjectAssembly projectAssembly,
			IReadOnlyCollection<string> referenceList,
			_ISourceInformationProvider? sourceInformationProvider = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			Guard.ArgumentNotNull(nameof(assemblyInfo), assemblyInfo);
			Guard.ArgumentNotNull(nameof(projectAssembly), projectAssembly);
			Guard.ArgumentNotNull(nameof(referenceList), referenceList);

			var innerDiscoverer = default(IFrontControllerDiscoverer);
			var assemblyFileName = projectAssembly.AssemblyFilename;

			if (diagnosticMessageSink == null)
				diagnosticMessageSink = new _NullMessageSink();

			if (sourceInformationProvider == null)
			{
				sourceInformationProvider = _NullSourceInformationProvider.Instance;
#if NETFRAMEWORK
				if (assemblyFileName != null)
					sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName, diagnosticMessageSink);
#endif
			}

			var v2PathPattern = new Regex(@"^xunit\.execution\..*\.dll$");
			var v2ExecutionReference = referenceList.FirstOrDefault(reference => v2PathPattern.IsMatch(Path.GetFileNameWithoutExtension(reference)));
			if (v2ExecutionReference != null)
				innerDiscoverer = Xunit2.ForDiscovery(assemblyInfo, projectAssembly, v2ExecutionReference, sourceInformationProvider, diagnosticMessageSink);

#if NETFRAMEWORK
			if (referenceList.Any(reference => Path.GetFileNameWithoutExtension(reference) == "xunit.dll"))
				innerDiscoverer = Xunit1.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink);
#endif

			if (innerDiscoverer == null)
				throw new InvalidOperationException($"Unknown test framework: could not find xunit.dll (v1) or xunit.execution.*.dll (v2) in assembly reference list");

			return new XunitFrontController(innerDiscoverer);
		}

		/// <summary>
		/// Returns an implementation of <see cref="IFrontController"/> which can be used
		/// for both discovery and execution.
		/// </summary>
		/// <param name="projectAssembly">The test project assembly.</param>
		/// <param name="sourceInformationProvider">The source information provider. If <c>null</c>, uses the default (<see cref="T:Xunit.VisualStudioSourceInformationProvider"/>).</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public static IFrontController ForDiscoveryAndExecution(
			XunitProjectAssembly projectAssembly,
			_ISourceInformationProvider? sourceInformationProvider = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			Guard.ArgumentNotNull(nameof(projectAssembly), projectAssembly);

			var innerController = default(IFrontController);
			var assemblyFileName = projectAssembly.AssemblyFilename;
			var assemblyFolder = Path.GetDirectoryName(assemblyFileName);

			if (diagnosticMessageSink == null)
				diagnosticMessageSink = new _NullMessageSink();

#if NETFRAMEWORK
			if (sourceInformationProvider == null)
			{
				if (assemblyFileName == null)
					sourceInformationProvider = _NullSourceInformationProvider.Instance;
				else
					sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName, diagnosticMessageSink);
			}

			if (assemblyFolder != null)
			{
				if (Directory.EnumerateFiles(assemblyFolder, "xunit.execution.*.dll").Any())
					innerController = Xunit2.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink);
				else
				{
					var xunitPath = Path.Combine(assemblyFolder, "xunit.dll");
					if (File.Exists(xunitPath))
						innerController = Xunit1.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink);
				}
			}
#else
			if (sourceInformationProvider == null)
				sourceInformationProvider = _NullSourceInformationProvider.Instance;

			innerController = Xunit2.ForDiscoveryAndExecution(projectAssembly, sourceInformationProvider, diagnosticMessageSink);
#endif

			if (innerController == null)
				throw new InvalidOperationException($"Unknown test framework: could not find xunit.dll (v1) or xunit.execution.*.dll (v2) in {assemblyFolder ?? "<unknown assembly folder>"}");

			return new XunitFrontController(innerController);
		}
	}
}
