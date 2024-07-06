using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Indicates an attribute which is involved in test method interception (allows code to be run
/// before and after a test is run).
/// </summary>
public interface IBeforeAfterTestAttribute
{
	/// <summary>
	/// This method is called after the test method is executed.
	/// </summary>
	/// <param name="methodUnderTest">The method under test</param>
	/// <param name="test">The current <see cref="ITest"/></param>
	ValueTask After(
		MethodInfo methodUnderTest,
		IXunitTest test);

	/// <summary>
	/// This method is called before the test method is executed.
	/// </summary>
	/// <param name="methodUnderTest">The method under test</param>
	/// <param name="test">The current <see cref="ITest"/></param>
	ValueTask Before(
		MethodInfo methodUnderTest,
		IXunitTest test);
}
