namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ITestContextAccessor"/>.
/// </summary>
public class TestContextAccessor : ITestContextAccessor
{
	TestContextAccessor()
	{ }

	/// <summary>
	/// Get the singleton instance of <see cref="TestContextAccessor"/>.
	/// </summary>
	public static TestContextAccessor Instance = new();

	/// <inheritdoc/>
	public ITestContext Current => TestContext.Current;
}
