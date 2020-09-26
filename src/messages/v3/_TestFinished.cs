#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// This message indicates that a test has finished executing.
	/// </summary>
	public class _TestFinished : _TestResultMessage
	{ }
}
