using System;

namespace Xunit.v3;

/// <summary>
/// Used to declare a specific test collection for a test class. Only valid on test classes, and only
/// a single instance of a collection attribute may be present.
/// </summary>
public interface ICollectionAttribute
{
	/// <summary>
	/// Gets the name of the collection.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets the collection definition type. Returns <c>null</c> if the collection is purely
	/// based on name.
	/// </summary>
	Type? Type { get; }
}
