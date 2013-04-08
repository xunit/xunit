using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="IFrontController"/> which supports running tests from
    /// both xUnit.net v1 and v2.
    /// </summary>
    public class XunitFrontController : IFrontController
    {
        readonly Xunit2 xunit2;

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
            xunit2 = new Xunit2(assemblyFileName, configFileName, shadowCopy);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            xunit2.SafeDispose();
        }

        /// <inheritdoc/>
        public virtual void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            xunit2.Find(includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public virtual void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink)
        {
            xunit2.Find(typeName, includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public virtual void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            xunit2.Run(testMethods, messageSink);
        }
    }
}