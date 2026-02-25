using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a class which can generate test cases.
/// </summary>
public interface ICodeGenTestCaseFactory
{
	/// <summary>
	/// Generate test cases.
	/// </summary>
	/// <param name="discoveryOptions">The discovery options, which may influence test case metadata</param>
	/// <param name="testMethod">The test method that this test case belongs to</param>
	/// <param name="disposalTracker">The disposal tracker (typically used for class data instances and data in test rows)</param>
	ValueTask<IReadOnlyCollection<ICodeGenTestCase>> Generate(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestMethod testMethod,
		DisposalTracker disposalTracker);
}
