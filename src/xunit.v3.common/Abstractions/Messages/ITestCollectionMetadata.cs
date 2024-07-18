using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test collection.
/// </summary>
public interface ITestCollectionMetadata
{
	/// <summary>
	/// Gets the type that the test collection was defined with, if available; may be <c>null</c>
	/// if the test collection didn't have a definition type.
	/// </summary>
	string? TestCollectionClassName { get; }

	/// <summary>
	/// Gets the display name of the test collection.
	/// </summary>
	string TestCollectionDisplayName { get; }

	/// <summary>
	/// Gets the trait values associated with this test collection (and the test assembly).
	/// If there are none, or the framework does not support traits, this returns an empty
	/// dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets the unique ID for this test collection.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test collection should be able to discriminate among test collections
	/// in the same test assembly. This identifier should remain stable until such time as the developer
	/// changes some fundamental part of the identity (the test assembly, the collection definition
	/// class, or the collection name). Recompilation of the test assembly is reasonable as a stability
	/// changing event.
	/// </remarks>
	string UniqueID { get; }
}
