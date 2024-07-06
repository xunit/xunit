using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IBeforeAfterTestAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public abstract class BeforeAfterTestAttribute : Attribute, IBeforeAfterTestAttribute
{
	/// <inheritdoc/>
	public virtual ValueTask After(
		MethodInfo methodUnderTest,
		IXunitTest test) =>
			default;

	/// <inheritdoc/>
	public virtual ValueTask Before(
		MethodInfo methodUnderTest,
		IXunitTest test) =>
			default;
}
