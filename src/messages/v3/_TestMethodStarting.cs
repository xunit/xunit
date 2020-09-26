#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test method is about to begin executing.
	/// </summary>
	public class _TestMethodStarting : _TestMethodMessage
	{ }
}
