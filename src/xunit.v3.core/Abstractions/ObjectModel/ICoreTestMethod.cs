using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test method from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTestMethod : ITestMethod
{
	/// <summary>
	/// Gets the arity (number of generic types) of the test method.
	/// </summary>
	new int MethodArity { get; }

	/// <summary>
	/// Gets the test case orderer for the test method, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	new ICoreTestClass TestClass { get; }
}
