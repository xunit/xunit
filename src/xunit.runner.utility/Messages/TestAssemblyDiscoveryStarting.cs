using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Default implementation of <see cref="ITestAssemblyDiscoveryStarting"/>.
    /// </summary>
    public class TestAssemblyDiscoveryStarting : ITestAssemblyDiscoveryStarting, IMessageSinkMessageWithTypes
    {
        static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestAssemblyDiscoveryStarting).GetInterfaces().Select(x => x.FullName));

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssemblyDiscoveryStarting"/> class.
        /// </summary>
        /// <param name="assembly">Information about the assembly that is being discovered</param>
        /// <param name="appDomain">Indicates whether the tests will be discovered and run in a separate app domain</param>
        /// <param name="shadowCopy">Indicates whether shadow copying is being used</param>
        /// <param name="discoveryOptions">The discovery options</param>
        public TestAssemblyDiscoveryStarting(XunitProjectAssembly assembly,
                                             bool appDomain,
                                             bool shadowCopy,
                                             ITestFrameworkDiscoveryOptions discoveryOptions)
        {
            Assembly = assembly;
            AppDomain = appDomain;
            DiscoveryOptions = discoveryOptions;
            ShadowCopy = shadowCopy;
        }

        /// <inheritdoc/>
        public bool AppDomain { get; private set; }

        /// <inheritdoc/>
        public XunitProjectAssembly Assembly { get; private set; }

        /// <inheritdoc/>
        public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; private set; }

        /// <inheritdoc/>
        public HashSet<string> InterfaceTypes => interfaceTypes;

        /// <inheritdoc/>
        public bool ShadowCopy { get; private set; }
    }
}
