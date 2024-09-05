using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test from xUnit.net v3 based on reflection.
/// </summary>
public interface IXunitTest : ITest
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
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] TestMethodArguments { get; }

	/// <summary>
	/// Gets the timeout for the test, in milliseconds; if <c>0</c>, there is no timeout.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with <see cref="ParallelAlgorithm.Aggressive"/> will result
	/// in undefined behavior. Timeout is only supported by <see cref="ParallelAlgorithm.Conservative"/>
	/// (or when parallelization is disabled completely).
	/// </remarks>
	int Timeout { get; }
}
