using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Indicates an attribute which is involved in test interception (allows code to be run
/// before and after a test is run).
/// </summary>
/// <remarks>BeforeAfterTest attributes are only valid at the assembly, method, or class level.</remarks>
public interface IBeforeAfterTestAttribute
{
	/// <summary>
	/// This method is called after the test is executed.
	/// </summary>
	/// <param name="methodUnderTest">The method under test</param>
	/// <param name="test">The current test</param>
	void After(
		MethodInfo methodUnderTest,
		IXunitTest test);

	/// <summary>
	/// This method is called before the test is executed.
	/// </summary>
	/// <param name="methodUnderTest">The method under test</param>
	/// <param name="test">The current test</param>
	void Before(
		MethodInfo methodUnderTest,
		IXunitTest test);
}
