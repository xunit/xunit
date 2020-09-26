#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test class is about to begin executing.
	/// </summary>
	public class _TestClassStarting : _TestClassMessage
	{ }
}
