using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xunit2 tests, using a reflection-based
    /// implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public sealed class Xunit2 : Xunit2Discoverer, ITestFrameworkExecutor
    {
        readonly ITestFrameworkExecutor executor;

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