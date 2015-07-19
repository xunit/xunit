using System;
using System.Collections.Generic;
using System.IO;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="IFrontController"/> which supports running tests from
    /// both xUnit.net v1 and v2.
    /// </summary>
    public class XunitFrontController : IFrontController
    {
        readonly string assemblyFileName;
        readonly string configFileName;
        readonly IMessageSink diagnosticMessageSink;
        IFrontController innerController;
        readonly bool shadowCopy;
        private readonly string shadowCopyFolder;
        readonly ISourceInformationProvider sourceInformationProvider;
        readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();
        readonly bool useAppDomain;

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected XunitFrontController() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitFrontController"/> class.
        /// </summary>
        /// <param name="useAppDomain">Determines whether tests should be run in a separate app domain.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        /// <param name="sourceInformationProvider">The source information provider. If <c>null</c>, uses the default (<see cref="T:Xunit.VisualStudioSourceInformationProvider"/>).</param>
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        public XunitFrontController(bool useAppDomain,
                                    string assemblyFileName,
                                    string configFileName = null,
                                    bool shadowCopy = true,
                                    string shadowCopyFolder = null,
                                    ISourceInformationProvider sourceInformationProvider = null,
                                    IMessageSink diagnosticMessageSink = null)
        {
            this.useAppDomain = useAppDomain;
            this.assemblyFileName = assemblyFileName;
            this.configFileName = configFileName;
            this.shadowCopy = shadowCopy;
            this.shadowCopyFolder = shadowCopyFolder;
            this.sourceInformationProvider = sourceInformationProvider;
            this.diagnosticMessageSink = diagnosticMessageSink ?? new NullMessageSink();

            Guard.FileExists("assemblyFileName", assemblyFileName);

            if (this.sourceInformationProvider == null)
            {
#if !XAMARIN && !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !DNX451 && !DNXCORE50
                this.sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyFileName);
#else
                this.sourceInformationProvider = new NullSourceInformationProvider();
#endif
                toDispose.Push(this.sourceInformationProvider);
            }

        }

        private IFrontController InnerController
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
        public string TargetFramework
        {
            get { return InnerController.TargetFramework; }
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return InnerController.TestFrameworkDisplayName; }
        }

        /// <summary>
        /// FOR INTERNAL USE ONLY.
        /// </summary>
        protected virtual IFrontController CreateInnerController()
        {
            // TODO: Refactor this method -- too many ifdefs
#if !XAMARIN && !WINDOWS_PHONE_APP && !WINDOWS_PHONE
            var xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
#endif
            var xunitExecutionPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), ExecutionHelper.AssemblyFileName);

#if !ANDROID && !DNX451 && !DNXCORE50
            if (File.Exists(xunitExecutionPath))
#endif
                return new Xunit2(useAppDomain, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
#if !XAMARIN && !WINDOWS_PHONE_APP && !WINDOWS_PHONE && !DNX451 && !DNXCORE50
            if (File.Exists(xunitPath))
                return new Xunit1(useAppDomain, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
#endif

#if XAMARIN || WINDOWS_PHONE_APP || WINDOWS_PHONE
            throw new ArgumentException(string.Format("Unknown test framework: Could not find {0}", ExecutionHelper.AssemblyFileName), assemblyFileName);
#else
            throw new ArgumentException(string.Format("Unknown test framework: Could not find xunit.dll or {0}.", ExecutionHelper.AssemblyFileName), assemblyFileName);
#endif
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return InnerController.Deserialize(value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in toDispose)
                disposable.Dispose();
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
        {
            return InnerController.Serialize(testCase);
        }
    }
}
