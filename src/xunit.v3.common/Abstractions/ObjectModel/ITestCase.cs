using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk;

/// <summary>
/// Represents a single test case in the system. This test case usually represents a single test, but in
/// the case of dynamically generated data for data driven tests, the test case may actually return
/// multiple results when run.
/// </summary>
public interface ITestCase : ITestCaseMetadata
{
	/// <summary>
	/// Gets the test class that this test case belongs to; may be <c>null</c> if the test isn't backed by
	/// a class, but will not be <c>null</c> if <see cref="TestMethod"/> is not <c>null</c> (and must be
	/// the same instance returned via <see cref="TestMethod"/>).
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethod))]
	ITestClass? TestClass { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to. Must be the same instance returned
	/// via <see cref="TestMethod"/> and/or <see cref="TestClass"/> when they are not <c>null</c>.
	/// </summary>
	ITestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to; may be <c>null</c> if the test isn't backed by
	/// a method.
	/// </summary>
	ITestMethod? TestMethod { get; }
}
