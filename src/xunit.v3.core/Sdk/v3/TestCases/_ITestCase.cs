using System.Diagnostics.CodeAnalysis;

namespace Xunit.v3;

/// <summary>
/// Represents a single test case in the system. This test case usually represents a single test, but in
/// the case of dynamically generated data for data driven tests, the test case may actually return
/// multiple results when run.
/// </summary>
public interface _ITestCase : _ITestCaseMetadata
{
	/// <summary>
	/// Gets the test class that this test case belongs to; may be <c>null</c> if the test isn't backed by
	/// a class, but will not be <c>null</c> if <see cref="TestMethod"/> is not <c>null</c> (and must be
	/// the same instance returned via <see cref="TestMethod"/>).
	/// </summary>
	[NotNullIfNotNull(nameof(TestMethod))]
	_ITestClass? TestClass { get; }

	/// <summary>
	/// Gets the test collection this test case belongs to. Must be the same instance returned
	/// via <see cref="TestMethod"/> and/or <see cref="TestClass"/> when they are not <c>null</c>.
	/// </summary>
	_ITestCollection TestCollection { get; }

	/// <summary>
	/// Gets the test method this test case belongs to; may be <c>null</c> if the test isn't backed by
	/// a method.
	/// </summary>
	_ITestMethod? TestMethod { get; }

	/// <summary>
	/// Gets the arguments that will be passed to the test method.
	/// </summary>
	object?[] TestMethodArguments { get; }

	/// <summary>
	/// Gets a unique identifier for the test case.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test case should be able to discriminate
	/// among test cases, even those which are varied invocations against the
	/// same test method (i.e., theories). Ideally, this identifier would remain
	/// stable until such time as the developer changes some fundamental part
	/// of the identity (assembly, class name, test name, or test data); however,
	/// the minimum stability of the identifier must at least extend across
	/// multiple discoveries of the same test in the same (non-recompiled)
	/// assembly.
	/// </remarks>
	string UniqueID { get; }
}
