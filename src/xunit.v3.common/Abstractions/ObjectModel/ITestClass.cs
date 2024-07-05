namespace Xunit.Sdk;

/// <summary>
/// Represents a test class, which contributes one or more test cases (usually by
/// way of test methods).
/// </summary>
/// <remarks>
/// Not all test frameworks will require that tests come from classes, so this abstraction
/// may or many not be used.
/// </remarks>
public interface ITestClass : ITestClassMetadata
{
	/// <summary>
	/// Gets the test collection this test class belongs to.
	/// </summary>
	ITestCollection TestCollection { get; }
}
