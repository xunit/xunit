using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

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
        ITestCaseBulkDeserializer bulkDeserializer;
        readonly string configFileName;
        ITestCaseDescriptorProvider descriptorProvider;
        readonly IMessageSink diagnosticMessageSink;
        IFrontController innerController;
        readonly bool shadowCopy;
        readonly string shadowCopyFolder;
        readonly ISourceInformationProvider sourceInformationProvider;
        readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected XunitFrontController() { }

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
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        public XunitFrontController(AppDomainSupport appDomainSupport,
                                    string assemblyFileName,
                                    string configFileName = null,
                                    bool shadowCopy = true,
                                    string shadowCopyFolder = null,
                                    ISourceInformationProvider sourceInformationProvider = null,
                                    IMessageSink diagnosticMessageSink = null)
        {
            this.appDomainSupport = appDomainSupport;
            this.assemblyFileName = assemblyFileName;
            this.configFileName = configFileName;
            this.shadowCopy = shadowCopy;
            this.shadowCopyFolder = shadowCopyFolder;
            this.sourceInformationProvider = sourceInformationProvider;
            this.diagnosticMessageSink = diagnosticMessageSink ?? new NullMessageSink();

            Guard.FileExists("assemblyFileName", assemblyFileName);

            if (this.sourceInformationProvider == null)
            {
#if NETSTANDARD
                this.sourceInformationProvider = new NullSourceInformationProvider();
#else
                this.sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName);
#endif
                toDispose.Push(this.sourceInformationProvider);
            }

        }

        ITestCaseBulkDeserializer BulkDeserializer
        {
            get
            {
                EnsureInitialized();
                return bulkDeserializer;
            }
        }

        /// <inheritdoc/>
        public bool CanUseAppDomains
            => InnerController.CanUseAppDomains;

        ITestCaseDescriptorProvider DescriptorProvider
        {
            get
            {
                EnsureInitialized();
                return descriptorProvider;
            }
        }

        IFrontController InnerController
        {
            get
            {
                EnsureInitialized();
                return innerController;
            }
        }

        /// <inheritdoc/>
        public string TargetFramework
        {
            get { return InnerController.TargetFramework; }
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return InnerController.TestFrameworkDisplayName; }
        }

        /// <inheritdoc/>
        public List<KeyValuePair<string, ITestCase>> BulkDeserialize(List<string> serializations)
            => BulkDeserializer.BulkDeserialize(serializations);

        /// <summary>
        /// FOR INTERNAL USE ONLY.
        /// </summary>
        protected virtual IFrontController CreateInnerController()
        {
#if NETFRAMEWORK
            var assemblyFolder = Path.GetDirectoryName(assemblyFileName);
#if NET35
            if (Directory.GetFiles(assemblyFolder, "xunit.execution.*.dll").Length > 0)
#else
            if (Directory.EnumerateFiles(assemblyFolder, "xunit.execution.*.dll").Any())
#endif
                return new Xunit2(appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);

            var xunitPath = Path.Combine(assemblyFolder, "xunit.dll");
            if (File.Exists(xunitPath))
                return new Xunit1(appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

            throw new InvalidOperationException($"Unknown test framework: could not find xunit.dll (v1) or xunit.execution.*.dll (v2) in {assemblyFolder}");
#else
            return new Xunit2(appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
#endif
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
            => InnerController.Deserialize(value);

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in toDispose)
                disposable.Dispose();
        }

        void EnsureInitialized()
        {
            if (innerController == null)
            {
                innerController = CreateInnerController();
                descriptorProvider = (innerController as ITestCaseDescriptorProvider) ?? new DefaultTestCaseDescriptorProvider(innerController);
                bulkDeserializer = (innerController as ITestCaseBulkDeserializer) ?? new DefaultTestCaseBulkDeserializer(innerController);
                toDispose.Push(innerController);
            }
        }

        /// <inheritdoc/>
        public virtual void Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            InnerController.Find(includeSourceInformation, messageSink, discoveryOptions);
        }

        /// <inheritdoc/>
        public virtual void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            InnerController.Find(typeName, includeSourceInformation, messageSink, discoveryOptions);
        }

        /// <inheritdoc/>
        public List<TestCaseDescriptor> GetTestCaseDescriptors(List<ITestCase> testCases, bool includeSerialization)
            => DescriptorProvider.GetTestCaseDescriptors(testCases, includeSerialization);

        /// <inheritdoc/>
        public virtual void RunAll(IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
        {
            InnerController.RunAll(messageSink, discoveryOptions, executionOptions);
        }

        /// <inheritdoc/>
        public virtual void RunTests(IEnumerable<ITestCase> testMethods, IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            InnerController.RunTests(testMethods, messageSink, executionOptions);
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
            => InnerController.Serialize(testCase);
    }
}
