using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a test method that should
/// be run by the default test runner. Implementations must be decorated by
/// <see cref="XunitTestCaseDiscovererAttribute"/> to indicate which class is responsible
/// for converting the test method into one or more tests.
/// </summary>
/// <remarks>The attribute can only be applied to methods, and only one attribute is allowed.</remarks>
public interface IFactAttribute
{
	/// <summary>
	/// Gets the name of the test to be used when the test is skipped. When <c>null</c>
	/// is returned, will cause a default display name to be used.
	/// </summary>
	string? DisplayName { get; }

	/// <summary>
	/// Gets a flag which indicates whether the test should only be run explicitly.
	/// An explicit test is skipped by default unless explicit tests are requested
	/// to be run.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the skip reason for the test. When <c>null</c> is returned, the test is
	/// not skipped.
	/// </summary>
	string? Skip { get; }

	// TODO: SkipWhen and SkipUnless, from https://github.com/xunit/xunit/issues/2339

	/// <summary>
	/// Gets the timeout for test (in milliseconds). When <c>0</c> is returned, the test
	/// will not have a timeout.
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with <see cref="ParallelAlgorithm.Aggressive"/> will result
	/// in undefined behavior. Test timing and timeouts are only reliable when using
	/// <see cref="ParallelAlgorithm.Conservative"/> (or when parallelization is disabled
	/// completely).
	/// </remarks>
	int Timeout { get; }
}
