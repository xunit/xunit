using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTest : ITest
{
	/// <summary>
	/// Gets a flag indicating whether this test was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the test case this test belongs to.
	/// </summary>
	new ICoreTestCase TestCase { get; }

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
