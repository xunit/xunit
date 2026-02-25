using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Indicates an attribute which is involved in test interception (allows code to be run
/// before and after a test is run).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BeforeAfterTestAttribute : Attribute, IBeforeAfterTestAttribute
{
	/// <inheritdoc/>
	public virtual void After(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{ }

	/// <inheritdoc/>
	public virtual void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{ }
}
