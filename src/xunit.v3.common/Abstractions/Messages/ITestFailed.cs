namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test has failed.
/// </summary>
public interface ITestFailed : ITestResultMessage, IErrorMetadata
{
	/// <summary>
	/// Gets the cause of the test failure.
	/// </summary>
	FailureCause Cause { get; }
}
