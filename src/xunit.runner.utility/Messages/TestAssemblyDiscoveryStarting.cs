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
        public TestAssemblyDiscoveryStarting(XunitProjectAssembly assembly,
                                             ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Assembly = assembly;
            DiscoveryOptions = discoveryOptions;
        }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }
    }
}
