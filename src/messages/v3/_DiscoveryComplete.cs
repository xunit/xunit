#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that the discovery process has been completed for
	/// the requested assembly.
	/// </summary>
	public class _DiscoveryComplete : _TestAssemblyMessage
	{ }
}
