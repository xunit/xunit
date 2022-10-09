namespace Xunit.Sdk;

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
	public TestContext? Current => TestContext.Current;
}
