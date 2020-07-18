using Xunit.Abstractions;

#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
using Xunit.Sdk;

namespace Xunit
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
