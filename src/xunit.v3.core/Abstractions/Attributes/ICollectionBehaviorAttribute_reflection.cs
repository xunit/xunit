namespace Xunit.v3;

/// <summary>
/// Used to declare the default test collection behavior for the assembly. This is only valid at the assembly level,
/// and there can be only one.
/// </summary>
public interface ICollectionBehaviorAttribute
{
	/// <summary>
	/// Gets the collection factory type specified by this collection behavior attribute.
	/// </summary>
	Type? CollectionFactoryType { get; }
}
