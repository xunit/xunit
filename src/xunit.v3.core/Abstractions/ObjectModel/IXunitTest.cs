using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTest : _ITest
{
	/// <summary>
	/// Gets a flag indicating whether this test was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the test case this test belongs to.
	/// </summary>
	new IXunitTestCase TestCase { get; }

	/// <summary>
	/// Gets the test method to run. May different from the test method embedded in the test case.
	/// </summary>
	IXunitTestMethod TestMethod { get; }

	/// <summary>
	/// A value greater than zero marks the test as having a timeout, and gets or sets the
	/// timeout (in milliseconds).
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with <see cref="ParallelAlgorithm.Aggressive"/> will result
	/// in undefined behavior. Timeout is only supported by <see cref="ParallelAlgorithm.Conservative"/>
	/// (or when parallelization is disabled completely).
	/// </remarks>
	int Timeout { get; }
}
