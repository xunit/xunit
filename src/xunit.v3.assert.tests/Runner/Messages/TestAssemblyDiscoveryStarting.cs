using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyDiscoveryStarting"/>.
    /// </summary>
    public class TestAssemblyDiscoveryStarting : ITestAssemblyDiscoveryStarting, IMessageSinkMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyDiscoveryStarting"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="appDomain">Indicates whether the tests will be discovered and run in a separate app domain</param>
        /// <param name="shadowCopy">Indicates whether shadow copying is being used</param>
        /// <param name="discoveryOptions">The discovery options</param>
        public TestAssemblyDiscoveryStarting(XunitProjectAssembly assembly,
                                             ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Assembly = assembly;
            DiscoveryOptions = discoveryOptions;
        }

        bool ITestAssemblyDiscoveryStarting.AppDomain => false;

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }

        bool ITestAssemblyDiscoveryStarting.ShadowCopy => false;

        XunitProjectAssembly ITestAssemblyDiscoveryStarting.Assembly => Assembly;
    }
}
