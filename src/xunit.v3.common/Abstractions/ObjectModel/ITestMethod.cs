namespace Xunit.Sdk;

/// <summary>
/// Represents a test method, which contributes one or more test cases.
/// </summary>
/// <remarks>
/// Not all test frameworks will require that tests come from methods, so this abstraction
/// may or many not be used.
/// </remarks>
public interface ITestMethod : ITestMethodMetadata
{
	/// <summary>
	/// Gets the test class that this test method belongs to.
	/// </summary>
	ITestClass TestClass { get; }
}
