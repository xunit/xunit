namespace Xunit.Sdk;

/// <summary>
/// Indicates the cause of the test failure.
/// </summary>
public enum FailureCause
{
	/// <summary>
	/// Indicates that a test failed for some reason other than a typical execution failure
	/// (for example, if a test was skipped but the flag was given to fail all skipped tests).
	/// </summary>
	Other = 0,

	/// <summary>
	/// Indicates that the test failed because it threw an unhandled exception.
	/// </summary>
	Exception = 1,

	/// <summary>
	/// Indicates that the test failed because of an assertion failure (that is, an exception
	/// was thrown that implements an interface named IAssertionException, regardless of the
	/// namespace or source assembly of the interface). For built-in exceptions, the
	/// <see cref="T:Xunit.Sdk.IAssertionException"/> serves this purpose, but this is generally
	/// found by convention rather than type to prevent 3rd party assertion libraries from needing
	/// to take an explicit references to xUnit.net binaries.
	/// </summary>
	Assertion = 2,

	/// <summary>
	/// Indicates that the test failed because it exceeded the allowed time to run (typically
	/// specified via <see cref="P:Xunit.FactAttribute.Timeout"/>). This is indicated by an
	/// exception that is thrown which implements an interface named ITestTimeoutException, regardless
	/// of the namespace or source assembly of the interface. For FactAttribute, the
	/// <see cref="T:Xunit.Sdk.ITestTimeoutException"/> serves this purpose, but this is generally
	/// found by convention rather than type to prevent 3rd party libraries from needing to
	/// take an explicit reference to xUnit.net binaries.
	/// </summary>
	Timeout = 3,
}
