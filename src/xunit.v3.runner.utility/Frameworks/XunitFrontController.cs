using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

#if NETFRAMEWORK
using System.Linq;
#endif

namespace Xunit
{
	/// <summary>
	/// Default implementation of <see cref="IFrontController"/> which supports running tests from
	/// both xUnit.net v1 and v2.
	/// </summary>
	public class XunitFrontController : IFrontController, IAsyncDisposable
	{
		readonly AppDomainSupport appDomainSupport;
		readonly string assemblyFileName;
		readonly string? configFileName;
		readonly _IMessageSink diagnosticMessageSink;
		readonly DisposalTracker disposalTracker = new DisposalTracker();
		bool disposed;
		IFrontController? innerController;
		readonly bool shadowCopy;
		readonly string? shadowCopyFolder;
		readonly _ISourceInformationProvider sourceInformationProvider;

		/// <summary>
		/// This constructor is for unit testing purposes only.
		/// </summary>
		protected XunitFrontController()
		{
			assemblyFileName = "<test value>";
			diagnosticMessageSink = new _NullMessageSink();
			sourceInformationProvider = _NullSourceInformationProvider.Instance;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitFrontController"/> class.
		/// </summary>
		/// <param name="appDomainSupport">Determines whether tests should be run in a separate app domain.</param>
		/// <param name="assemblyFileName">The test assembly.</param>
		/// <param name="configFileName">The test assembly configuration file.</param>
		/// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
		/// tests to be discovered and run without locking assembly files on disk.</param>
		/// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
		/// will be automatically (randomly) generated</param>
		/// <param name="sourceInformationProvider">The source information provider. If <c>null</c>, uses the default (<see cref="T:Xunit.VisualStudioSourceInformationProvider"/>).</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public XunitFrontController(
			AppDomainSupport appDomainSupport,
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null,
			_ISourceInformationProvider? sourceInformationProvider = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			this.appDomainSupport = appDomainSupport;
			this.assemblyFileName = assemblyFileName;
			this.configFileName = configFileName;
			this.shadowCopy = shadowCopy;
			this.shadowCopyFolder = shadowCopyFolder;
			this.diagnosticMessageSink = diagnosticMessageSink ?? new _NullMessageSink();

			Guard.FileExists("assemblyFileName", assemblyFileName);

			if (sourceInformationProvider == null)
			{
#if NETSTANDARD
				this.sourceInformationProvider = _NullSourceInformationProvider.Instance;
#else
				this.sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName, this.diagnosticMessageSink);
				disposalTracker.Add(this.sourceInformationProvider);
#endif
			}
			else
			{
				this.sourceInformationProvider = sourceInformationProvider;
			}
		}

		/// <inheritdoc/>
		public bool CanUseAppDomains => InnerController.CanUseAppDomains;

		IFrontController InnerController
		{
			get
			{
				if (innerController == null)
				{
					innerController = CreateInnerController();
					disposalTracker.Add(innerController);
				}

				return innerController;
			}
		}

		/// <inheritdoc/>
		public string TargetFramework => InnerController.TargetFramework;

		/// <inheritdoc/>
		public string TestAssemblyUniqueID => InnerController.TestAssemblyUniqueID;

		/// <inheritdoc/>
		public string TestFrameworkDisplayName => InnerController.TestFrameworkDisplayName;

		/// <summary>
		/// FOR INTERNAL USE ONLY.
		/// </summary>
		protected virtual IFrontController CreateInnerController()
		{
#if NETFRAMEWORK
			var assemblyFolder = Path.GetDirectoryName(assemblyFileName)!;
			if (Directory.EnumerateFiles(assemblyFolder, "xunit.execution.*.dll").Any())
				return new Xunit2(diagnosticMessageSink, appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

			var xunitPath = Path.Combine(assemblyFolder, "xunit.dll");
			if (File.Exists(xunitPath))
				return new Xunit1(diagnosticMessageSink, appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

			throw new InvalidOperationException($"Unknown test framework: could not find xunit.dll (v1) or xunit.execution.*.dll (v2) in {assemblyFolder}");
#else
			return new Xunit2(diagnosticMessageSink, appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
#endif
		}

		/// <inheritdoc/>
		public ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			return disposalTracker.DisposeAsync();
		}

		/// <inheritdoc/>
		public virtual void Find(
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			InnerController.Find(messageSink, discoveryOptions);
		}

		/// <inheritdoc/>
		public virtual void Find(
			string typeName,
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(typeName), typeName);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			InnerController.Find(typeName, messageSink, discoveryOptions);
		}

		/// <inheritdoc/>
		public virtual void RunAll(
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			InnerController.RunAll(messageSink, discoveryOptions, executionOptions);
		}

		/// <inheritdoc/>
		public virtual void RunTests(
			IEnumerable<_ITestCase> testMethods,
			_IMessageSink messageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(testMethods), testMethods);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			InnerController.RunTests(testMethods, messageSink, executionOptions);
		}

		/// <inheritdoc/>
		public virtual void RunTests(
			IEnumerable<string> serializedTestCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(serializedTestCases), serializedTestCases);
			Guard.ArgumentNotNull(nameof(executionMessageSink), executionMessageSink);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			InnerController.RunTests(serializedTestCases, executionMessageSink, executionOptions);
		}
	}
}
