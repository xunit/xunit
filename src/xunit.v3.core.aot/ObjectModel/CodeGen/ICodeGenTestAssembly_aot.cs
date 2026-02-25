namespace Xunit.v3;

/// <summary>
/// Represents a test assembly from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTestAssembly : ICoreTestAssembly
{
	/// <summary>
	/// Gets the fixture factories for the assembly-level test fixtures.
	/// </summary>
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> AssemblyFixtureFactories { get; }

	/// <summary>
	/// Gets the <see cref="BeforeAfterTestAttribute"/>s attached to the test assembly.
	/// </summary>
	IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the collection definitions attached to the test assembly, by collection name.
	/// </summary>
	IReadOnlyDictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitions { get; }

	/// <summary>
	/// Gets the test collection factory for the test assembly.
	/// </summary>
	ICodeGenTestCollectionFactory TestCollectionFactory { get; }
}
