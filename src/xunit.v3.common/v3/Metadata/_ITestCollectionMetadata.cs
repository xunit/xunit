namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test collection.
/// </summary>
public interface _ITestCollectionMetadata
{
	/// <summary>
	/// Gets the type that the test collection was defined with, if available; may be <c>null</c>
	/// if the test collection didn't have a definition type.
	/// </summary>
	string? TestCollectionClass { get; }

	/// <summary>
	/// Gets the display name of the test collection.
	/// </summary>
	string TestCollectionDisplayName { get; }
}
