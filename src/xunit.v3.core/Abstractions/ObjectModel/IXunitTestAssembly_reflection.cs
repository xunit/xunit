namespace Xunit.v3;

/// <summary>
/// Represents a test assembly from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for reflection-based tests.
/// </remarks>
public interface IXunitTestAssembly : ICoreTestAssembly
{
	/// <summary>
	/// Gets a list of fixture types associated with the test assembly.
	/// </summary>
	IReadOnlyCollection<Type> AssemblyFixtureTypes { get; }

	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the test assembly.
	/// </summary>
	IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the collection behavior associated with the assembly, if present.
	/// </summary>
	ICollectionBehaviorAttribute? CollectionBehavior { get; }

	/// <summary>
	/// Gets the collection definitions attached to the test assembly, by collection name.
	/// </summary>
	IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> CollectionDefinitions { get; }
}
