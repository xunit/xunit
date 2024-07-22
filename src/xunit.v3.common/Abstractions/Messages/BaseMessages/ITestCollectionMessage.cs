namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to test collections.
/// </summary>
public interface ITestCollectionMessage : ITestAssemblyMessage
{
	/// <summary>
	/// Gets the test collection's unique ID. Can be used to correlate test messages with the appropriate
	/// test collection that they're related to.
	/// </summary>
	string TestCollectionUniqueID { get; }
}
