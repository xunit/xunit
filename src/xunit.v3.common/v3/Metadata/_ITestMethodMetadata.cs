namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test method.
/// </summary>
public interface _ITestMethodMetadata
{
	/// <summary>
	/// Gets the name of the test method that is associated with this message.
	/// </summary>
	string TestMethod { get; }
}
