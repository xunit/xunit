using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test collection from xUnit.net v3.
/// </summary>
/// <remarks>
/// Test collections form the basis of the parallelization in xUnit.net v3. Test cases
/// which are in the same test collection will not be run in parallel against sibling
/// tests, but will run in parallel against tests in other collections. They also provide
/// a level of shared context via <see cref="ICollectionFixture{TFixture}"/>.<br />
/// <br />
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTestCollection : ITestCollection
{
	/// <summary>
	/// Determines whether tests in this collection runs in parallel with any other collections.
	/// </summary>
	bool DisableParallelization { get; }

	/// <summary>
	/// Gets the test assembly this test collection belongs to.
	/// </summary>
	new ICoreTestAssembly TestAssembly { get; }

	/// <summary>
	/// Gets the test case orderer for the test collection, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test class orderer for the test collection, if present.
	/// </summary>
	ITestClassOrderer? TestClassOrderer { get; }

	/// <summary>
	/// Gets the test method orderer for the test collection, if present.
	/// </summary>
	ITestMethodOrderer? TestMethodOrderer { get; }
}
