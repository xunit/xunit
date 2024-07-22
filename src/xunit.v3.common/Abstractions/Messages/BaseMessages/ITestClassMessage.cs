namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to test classes.
/// </summary>
public interface ITestClassMessage : ITestCollectionMessage
{
	/// <summary>
	/// Gets the test class's unique ID. Can be used to correlate test messages with the appropriate
	/// test class that they're related to. Will be <c>null</c> if the test did not originate from a class.
	/// </summary>
	string? TestClassUniqueID { get; }
}
