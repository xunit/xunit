namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test class.
/// </summary>
public interface _ITestClassMetadata
{
	/// <summary>
	/// Gets the fully qualified type name of the test class that is associated with this message.
	/// </summary>
	string TestClass { get; }
}
