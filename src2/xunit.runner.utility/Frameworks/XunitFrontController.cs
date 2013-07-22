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
        readonly IFrontController innerController;

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
        /// tests to be discovered and run without locking assembly files on disk.</param>
        public XunitFrontController(string assemblyFileName, string configFileName = null, bool shadowCopy = true)
        {
            Guard.FileExists("assemblyFileName", assemblyFileName);
            var xunit1Path = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll");
            var xunit2Path = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");
            var sourceInformationProvider = new VisualStudioSourceInformationProvider();

            if (File.Exists(xunit2Path))
                innerController = new Xunit2(sourceInformationProvider, assemblyFileName, configFileName, shadowCopy);
            else
                throw new ArgumentException("Unknown test framework: Could not find xunit.dll or xunit2.dll.", assemblyFileName);
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return innerController.TestFrameworkDisplayName; }
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return innerController.Deserialize(value);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            innerController.SafeDispose();
        }

        /// <inheritdoc/>
        public virtual void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            innerController.Find(includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public virtual void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink)
        {
            innerController.Find(typeName, includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public virtual void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            innerController.Run(testMethods, messageSink);
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            return innerController.Serialize(testCase);
        }
    }
}