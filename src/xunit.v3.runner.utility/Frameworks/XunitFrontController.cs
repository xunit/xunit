using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;
using Xunit.Runner.Common;

#if NETFRAMEWORK
using System.Linq;
#endif

namespace Xunit
{
	/// <summary>
	/// Default implementation of <see cref="IFrontController"/> which supports running tests from
	/// both xUnit.net v1 and v2.
	/// </summary>
	public class XunitFrontController : IFrontController, ITestCaseDescriptorProvider, ITestCaseBulkDeserializer
	{
		readonly AppDomainSupport appDomainSupport;
		readonly string assemblyFileName;
		ITestCaseBulkDeserializer? bulkDeserializer;
		readonly string? configFileName;
		readonly IMessageSink diagnosticMessageSink;
		bool disposed;
		ITestCaseDescriptorProvider? descriptorProvider;
		IFrontController? innerController;
		readonly bool shadowCopy;
		readonly string? shadowCopyFolder;
		readonly ISourceInformationProvider sourceInformationProvider;
		readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();

		/// <summary>
		/// This constructor is for unit testing purposes only.
		/// </summary>
		protected XunitFrontController()
		{
			assemblyFileName = "<test value>";
			diagnosticMessageSink = new NullMessageSink();
			sourceInformationProvider = new NullSourceInformationProvider();
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
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		public XunitFrontController(
			AppDomainSupport appDomainSupport,
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null,
			ISourceInformationProvider? sourceInformationProvider = null,
			IMessageSink? diagnosticMessageSink = null)
		{
			this.appDomainSupport = appDomainSupport;
			this.assemblyFileName = assemblyFileName;
			this.configFileName = configFileName;
			this.shadowCopy = shadowCopy;
			this.shadowCopyFolder = shadowCopyFolder;
			this.diagnosticMessageSink = diagnosticMessageSink ?? new NullMessageSink();

			Guard.FileExists("assemblyFileName", assemblyFileName);

			if (sourceInformationProvider == null)
			{
#if NETSTANDARD
				this.sourceInformationProvider = new NullSourceInformationProvider();
#else
				this.sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName, this.diagnosticMessageSink);
				toDispose.Push(this.sourceInformationProvider);
#endif
			}
			else
			{
				this.sourceInformationProvider = sourceInformationProvider;
			}
		}

		ITestCaseBulkDeserializer BulkDeserializer
		{
			get
			{
				if (bulkDeserializer == null)
					bulkDeserializer = (InnerController as ITestCaseBulkDeserializer) ?? new DefaultTestCaseBulkDeserializer(InnerController);

				return bulkDeserializer;
			}
		}

		/// <inheritdoc/>
		public bool CanUseAppDomains => InnerController.CanUseAppDomains;

		ITestCaseDescriptorProvider DescriptorProvider
		{
			get
			{
				if (descriptorProvider == null)
					descriptorProvider = (InnerController as ITestCaseDescriptorProvider) ?? new DefaultTestCaseDescriptorProvider(InnerController);

				return descriptorProvider;
			}
		}

		IFrontController InnerController
		{
			get
			{
				if (innerController == null)
				{
					innerController = CreateInnerController();
					toDispose.Push(innerController);
				}

				return innerController;
			}
		}

		/// <inheritdoc/>
		public string TargetFramework => InnerController.TargetFramework;

		/// <inheritdoc/>
		public string TestFrameworkDisplayName => InnerController.TestFrameworkDisplayName;

		/// <inheritdoc/>
		public List<KeyValuePair<string, ITestCase>> BulkDeserialize(List<string> serializations) =>
			BulkDeserializer.BulkDeserialize(serializations);

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
		public ITestCase Deserialize(string value)
		{
			Guard.ArgumentNotNull(nameof(value), value);

			return InnerController.Deserialize(value);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			foreach (var disposable in toDispose)
				disposable.Dispose();
		}

		/// <inheritdoc/>
		public virtual void Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			InnerController.Find(includeSourceInformation, messageSink, discoveryOptions);
		}

		/// <inheritdoc/>
		public virtual void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(typeName), typeName);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			InnerController.Find(typeName, includeSourceInformation, messageSink, discoveryOptions);
		}

		/// <inheritdoc/>
		public List<TestCaseDescriptor> GetTestCaseDescriptors(List<ITestCase> testCases, bool includeSerialization) =>
			DescriptorProvider.GetTestCaseDescriptors(testCases, includeSerialization);

		/// <inheritdoc/>
		public virtual void RunAll(IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			InnerController.RunAll(messageSink, discoveryOptions, executionOptions);
		}

		/// <inheritdoc/>
		public virtual void RunTests(IEnumerable<ITestCase> testMethods, IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
		{
			Guard.ArgumentNotNull(nameof(testMethods), testMethods);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(executionOptions), executionOptions);

			InnerController.RunTests(testMethods, messageSink, executionOptions);
		}

		/// <inheritdoc/>
		public string Serialize(ITestCase testCase)
		{
			Guard.ArgumentNotNull(nameof(testCase), testCase);

			return InnerController.Serialize(testCase);
		}
	}
}
