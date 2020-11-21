using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

// TODO: These will be replaced by their counterparts in xunit.v3.common/v3/Messages once we replace the message sink.
namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ITestAssemblyDiscoveryStarting"/>.
	/// </summary>
	public class TestAssemblyDiscoveryStarting : ITestAssemblyDiscoveryStarting, IMessageSinkMessageWithTypes
	{
		static readonly HashSet<string> interfaceTypes = new HashSet<string>(typeof(TestAssemblyDiscoveryStarting).GetInterfaces().Select(x => x.FullName!));

		/// <summary>
		/// Initializes a new instance of the <see cref="TestAssemblyDiscoveryStarting"/> class.
		/// </summary>
		/// <param name="assembly">Information about the assembly that is being discovered</param>
		/// <param name="appDomain">Indicates whether the tests will be discovered and run in a separate app domain</param>
		/// <param name="shadowCopy">Indicates whether shadow copying is being used</param>
		/// <param name="discoveryOptions">The discovery options</param>
		public TestAssemblyDiscoveryStarting(
			XunitProjectAssembly assembly,
			AppDomainOption appDomain,
			bool shadowCopy,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);
			Guard.ArgumentNotNull(nameof(discoveryOptions), discoveryOptions);

			Assembly = assembly;
			AppDomain = appDomain;
			DiscoveryOptions = discoveryOptions;
			ShadowCopy = shadowCopy;
		}

		/// <inheritdoc/>
		public AppDomainOption AppDomain { get; }

		/// <inheritdoc/>
		public XunitProjectAssembly Assembly { get; }

		/// <inheritdoc/>
		public _ITestFrameworkDiscoveryOptions DiscoveryOptions { get; }

		/// <inheritdoc/>
		public HashSet<string> InterfaceTypes => interfaceTypes;

		/// <inheritdoc/>
		public bool ShadowCopy { get; }
	}
}
