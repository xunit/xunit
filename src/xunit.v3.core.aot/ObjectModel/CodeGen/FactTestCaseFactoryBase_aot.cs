namespace Xunit.v3;

/// <summary>
/// A base implementation of <see cref="ICodeGenTestCaseFactory"/> designed to support
/// <see cref="FactAttribute"/> and <see cref="CulturedFactAttribute"/>.
/// </summary>
public abstract class FactTestCaseFactoryBase : TestCaseFactoryBase
{
	/// <summary>
	/// Gets the function which invokes the test method when the test runs.
	/// </summary>
	public required Func<object?, ValueTask> MethodInvoker { get; init; }
}
