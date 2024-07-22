namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test was skipped.
/// </summary>
public interface ITestSkipped : ITestResultMessage
{
	/// <summary>
	/// Gets the reason given for skipping the test.
	/// </summary>
	string Reason { get; }
}
