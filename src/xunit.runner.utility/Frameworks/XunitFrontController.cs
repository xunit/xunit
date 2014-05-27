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
        IFrontController innerController;
        readonly bool shadowCopy;
        readonly ISourceInformationProvider sourceInformationProvider;
        readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected XunitFrontController() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitFrontController"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// <param name="sourceInformationProvider">The source information provider. If <c>null</c>, uses the default (<see cref="T:Xunit.VisualStudioSourceInformationProvider"/>).</param>
        /// tests to be discovered and run without locking assembly files on disk.</param>
        public XunitFrontController(string assemblyFileName, string configFileName = null, bool shadowCopy = true, ISourceInformationProvider sourceInformationProvider = null)
        {
            this.assemblyFileName = assemblyFileName;
            this.configFileName = configFileName;
            this.shadowCopy = shadowCopy;
            this.sourceInformationProvider = sourceInformationProvider;

#if !ANDROID
            Guard.FileExists("assemblyFileName", assemblyFileName);
#endif

            if (this.sourceInformationProvider == null)
            {
#if !XAMARIN
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
#if !XAMARIN
            var xunitPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
#endif
            var xunitExecutionPath = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.execution.dll");

#if !ANDROID
            if (File.Exists(xunitExecutionPath))
#endif
                return new Xunit2(sourceInformationProvider, assemblyFileName, configFileName, shadowCopy);
#if !XAMARIN
            if (File.Exists(xunitPath))
                return new Xunit1(sourceInformationProvider, assemblyFileName, configFileName, shadowCopy);
#endif

#if XAMARIN
            throw new ArgumentException("Unknown test framework: Could not find xunit.execution.dll.", assemblyFileName);
#else
            throw new ArgumentException("Unknown test framework: Could not find xunit.dll or xunit.execution.dll.", assemblyFileName);
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
        public virtual void Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            InnerController.Find(includeSourceInformation, messageSink, options);
        }

        /// <inheritdoc/>
        public virtual void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            InnerController.Find(typeName, includeSourceInformation, messageSink, options);
        }

        /// <inheritdoc/>
        public virtual void RunAll(IMessageSink messageSink, ITestFrameworkOptions discoveryOptions, ITestFrameworkOptions executionOptions)
        {
            InnerController.RunAll(messageSink, discoveryOptions, executionOptions);
        }

        /// <inheritdoc/>
        public virtual void RunTests(IEnumerable<ITestCase> testMethods, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            InnerController.RunTests(testMethods, messageSink, options);
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return InnerController.Serialize(testCase);
        }
    }
}