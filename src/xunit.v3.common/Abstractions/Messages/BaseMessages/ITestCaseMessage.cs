namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to test cases.
/// </summary>
public interface ITestCaseMessage : ITestMethodMessage
{
	/// <summary>
	/// Gets the test case's unique ID. Can be used to correlate test messages with the appropriate
	/// test case that they're related to.
	/// </summary>
	string TestCaseUniqueID { get; }
}
