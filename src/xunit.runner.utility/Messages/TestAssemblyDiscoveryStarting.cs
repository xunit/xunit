using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyDiscoveryStarting"/>.
    /// </summary>
    public class TestAssemblyDiscoveryStarting : ITestAssemblyDiscoveryStarting
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyDiscoveryStarting"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="discoveryOptions">The discovery options</param>
        /// <param name="executionOptions">The execution options</param>
        public TestAssemblyDiscoveryStarting(XunitProjectAssembly assembly,
                                             ITestFrameworkDiscoveryOptions discoveryOptions,
                                             ITestFrameworkExecutionOptions executionOptions)
        {
            Assembly = assembly;
            DiscoveryOptions = discoveryOptions;
            ExecutionOptions = executionOptions;
        }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkExecutionOptions ExecutionOptions { get; private set; }
    }
}