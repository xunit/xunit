namespace Xunit.Sdk;

/// <summary>
/// Represents a group of test cases.
/// </summary>
/// <remarks>
/// The test framework decides how test collections are defined and what their purpose is.
/// </remarks>
public interface ITestCollection : ITestCollectionMetadata
{
	/// <summary>
	/// Gets the test assembly this test collection belongs to.
	/// </summary>
	ITestAssembly TestAssembly { get; }
}
