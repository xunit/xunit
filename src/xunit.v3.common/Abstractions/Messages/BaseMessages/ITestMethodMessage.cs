namespace Xunit.Sdk;

/// <summary>
/// Base interface for all messages related to test methods.
/// </summary>
public interface ITestMethodMessage : ITestClassMessage
{
	/// <summary>
	/// Gets the test method's unique ID. Can be used to correlate test messages with the appropriate
	/// test method that they're related to. Will be <c>null</c> if the test did not originate from a method.
	/// </summary>
	string? TestMethodUniqueID { get; }
}
