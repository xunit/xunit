namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to tests.
/// </summary>
public interface ITestMessage : ITestCaseMessage
{
	/// <summary>
	/// Gets the test's unique ID. Can be used to correlate test messages with the appropriate
	/// test that they're related to.
	/// </summary>
	string TestUniqueID { get; }
}
