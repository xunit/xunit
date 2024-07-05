namespace Xunit.Sdk;

/// <summary>
/// Represents a single test in the system. A test case typically contains only a single test,
/// but may contain many if circumstances warrant it (for example, test data for a theory cannot
/// be pre-enumerated, so the theory yields a single test case with multiple tests).
/// </summary>
public interface ITest : ITestMetadata
{
	/// <summary>
	/// Gets the test case this test belongs to.
	/// </summary>
	ITestCase TestCase { get; }
}
