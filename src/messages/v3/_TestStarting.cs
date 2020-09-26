#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test is about to start executing.
	/// </summary>
	public class _TestStarting : _TestMessage
	{ }
}
