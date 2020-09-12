using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.v2
#else
using Xunit.Sdk;

namespace Xunit.Runner.v2
#endif
{
	/// <summary>
	/// Default implementation of <see cref="IDiscoveryCompleteMessage"/>.
	/// </summary>
#if XUNIT_FRAMEWORK
	public class DiscoveryCompleteMessage : IDiscoveryCompleteMessage
#else
	public class DiscoveryCompleteMessage : LongLivedMarshalByRefObject, IDiscoveryCompleteMessage
#endif
	{ }
}
