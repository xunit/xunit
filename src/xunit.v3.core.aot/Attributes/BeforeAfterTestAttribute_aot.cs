namespace Xunit.v3;

/// <summary>
/// Indicates an attribute which is involved in test interception (allows code to be run
/// before and after a test is run).
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BeforeAfterTestAttribute : Attribute
{
	/// <summary>
	/// This method is called after the test is executed.
	/// </summary>
	/// <param name="test">The current test</param>
	public virtual void After(ICodeGenTest test)
	{ }

	/// <summary>
	/// This method is called before the test is executed.
	/// </summary>
	/// <param name="test">The current test</param>
	public virtual void Before(ICodeGenTest test)
	{ }
}
