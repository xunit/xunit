using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyDiscoveryFinished"/>.
    /// </summary>
    public class TestAssemblyDiscoveryFinished : ITestAssemblyDiscoveryFinished
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyDiscoveryFinished"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="discoveryOptions">The discovery options</param>
        /// <param name="executionOptions">The execution options</param>
        /// <param name="testCasesDiscovered">The number of test cases discovered</param>
        /// <param name="testCasesToRun">The number of test cases to be run</param>
        public TestAssemblyDiscoveryFinished(XunitProjectAssembly assembly,
                                             ITestFrameworkDiscoveryOptions discoveryOptions,
                                             ITestFrameworkExecutionOptions executionOptions,
                                             int testCasesDiscovered,
                                             int testCasesToRun)
        {
            Assembly = assembly;
            DiscoveryOptions = discoveryOptions;
            ExecutionOptions = executionOptions;
            TestCasesDiscovered = testCasesDiscovered;
            TestCasesToRun = testCasesToRun;
        }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkExecutionOptions ExecutionOptions { get; private set; }

        /// <inheritdoc/>
        public int TestCasesDiscovered { get; private set; }

        /// <inheritdoc/>
        public int TestCasesToRun { get; private set; }
    }
}