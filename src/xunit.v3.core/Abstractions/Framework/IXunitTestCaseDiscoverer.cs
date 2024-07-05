using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Interface to be implemented by classes which are used to discover tests cases attached
/// to test methods that are attributed with an implementation of <see cref="IFactAttribute"/>.
/// </summary>
public interface IXunitTestCaseDiscoverer
{
	/// <summary>
	/// Discover test cases from a test method.
	/// </summary>
	/// <param name="discoveryOptions">The discovery options to be used.</param>
	/// <param name="testMethod">The test method the test cases belong to.</param>
	/// <param name="factAttribute">The fact attribute attached to the test method.</param>
	/// <returns>Returns zero or more test cases represented by the test method.</returns>
	ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute
	);
}
