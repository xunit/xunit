#if XUNIT_FRAMEWORK
namespace Xunit.v3
#else
namespace Xunit.Runner.v3
#endif
{
	/// <summary>
	/// Indicates that a test has passed.
	/// </summary>
	public class _TestPassed : _TestResultMessage
	{ }
}
