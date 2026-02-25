namespace Xunit.v3;

/// <summary>
/// Represents a test collection from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTestCollection : ICoreTestCollection
{
	/// <summary>
	/// Gets the <see cref="BeforeAfterTestAttribute"/>s attached to the test collection (and
	/// the test assembly).
	/// </summary>
	IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the fixture factories for the class-level test fixtures on the test collection.
	/// </summary>
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; }

	/// <summary>
	/// Gets the fixture factories for the collection-level test fixtures.
	/// </summary>
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> CollectionFixtureFactories { get; }

	/// <summary>
	/// Gets the test assembly this test collection belongs to.
	/// </summary>
	new ICodeGenTestAssembly TestAssembly { get; }
}
