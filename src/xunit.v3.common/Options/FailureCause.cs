namespace Xunit.Sdk;

/// <summary>
/// Indicates the cause of the test failure.
/// </summary>
public enum FailureCause
{
	/// <summary>
	/// Indicates the test failure cause is unknown.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// Indicates that a test failed for some reason other than a typical execution failure
	/// (for example, if a test was skipped but the flag was given to fail all skipped tests,
	/// or the test passed with warnings but the flag was given to fail tests with warnings).
	/// </summary>
	Other = 1,

	/// <summary>
	/// Indicates that the test failed because it threw an unhandled exception.
	/// </summary>
	Exception = 2,

	/// <summary>
	/// Indicates that the test failed because of an assertion failure (that is, an exception
	/// was thrown that implements an interface named <c>IAssertionException</c>, regardless of the
	/// namespace or source assembly of the interface). For built-in exceptions,
	/// <see cref="T:Xunit.Sdk.IAssertionException"/> serves this purpose, but this is generally
	/// found by convention rather than type to prevent 3rd party assertion libraries from needing
	/// to take an explicit references to xUnit.net binaries.
	/// </summary>
	Assertion = 3,

	/// <summary>
	/// Indicates that the test failed because it exceeded the allowed time to run (typically
	/// specified via <see cref="P:Xunit.v3.IFactAttribute.Timeout"/>). This is indicated by an
	/// exception that is thrown which implements an interface named <c>ITestTimeoutException</c>,
	/// regardless of the namespace or source assembly of the interface. For fact attributes,
	/// <see cref="T:Xunit.v3.ITestTimeoutException"/> serves this purpose, but this is generally
	/// found by convention rather than type to prevent 3rd party libraries from needing to
	/// take an explicit reference to xUnit.net binaries.
	/// </summary>
	Timeout = 4,
}
