using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents a test class from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is shared between reflection-based and code generation-based tests.
/// </remarks>
public interface ICoreTestClass : ITestClass
{
	/// <summary>
	/// Gets the <see cref="Type"/> of this test class.
	/// </summary>
	Type Class { get; }

	/// <summary>
	/// Gets the test case orderer for the test class, if present.
	/// </summary>
	ITestCaseOrderer? TestCaseOrderer { get; }

	/// <summary>
	/// Gets the test collection this test class belongs to.
	/// </summary>
	new ICoreTestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method orderer for the test class, if present.
	/// </summary>
	ITestMethodOrderer? TestMethodOrderer { get; }
}
