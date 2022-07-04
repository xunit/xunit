namespace Xunit.v3;

/// <summary>
/// Represents a test method.
/// </summary>
public interface _ITestMethod
{
	/// <summary>
	/// Gets method information for the underlying test method.
	/// </summary>
	_IMethodInfo Method { get; }

	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	_ITestClass TestClass { get; }

	/// <summary>
	/// Gets the unique ID for this test method.
	/// </summary>
	string UniqueID { get; }
}
