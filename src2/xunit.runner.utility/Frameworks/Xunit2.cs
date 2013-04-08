using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v2 tests, using a reflection-based
    /// implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public sealed class Xunit2 : Xunit2Discoverer, ITestFrameworkExecutor
    {
        readonly ITestFrameworkExecutor executor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        public Xunit2(string assemblyFileName, string configFileName = null, bool shadowCopy = true)
            : base(assemblyFileName, configFileName, shadowCopy)
        {
            executor = Framework.GetExecutor(assemblyFileName);
        }

        /// <inheritdoc/>
        public override sealed void Dispose()
        {
            executor.SafeDispose();

            base.Dispose();
        }

        /// <inheritdoc/>
        public void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
        {
            executor.Run(testMethods, messageSink);
        }
    }
}