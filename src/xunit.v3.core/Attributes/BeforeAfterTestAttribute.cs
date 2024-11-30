using System;
using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IBeforeAfterTestAttribute"/>.
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
