using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    public class XunitFrontController : ITestFrameworkDiscoverer, ITestFrameworkExecutor
    {
        readonly Xunit2 xunit2;

        /// <summary>
        /// This constructor is for unit testing purposes only.
        /// </summary>
        protected XunitFrontController() { }

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
        public virtual void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
        {
            xunit2.Find(type, includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        public virtual void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            xunit2.Run(testMethods, messageSink);
        }
    }
}