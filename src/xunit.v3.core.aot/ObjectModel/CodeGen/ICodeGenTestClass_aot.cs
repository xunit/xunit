namespace Xunit.v3;

/// <summary>
/// Represents a test class from xUnit.net v3.
/// </summary>
/// <remarks>
/// This interface is used for code generation-based tests.
/// </remarks>
public interface ICodeGenTestClass : ICoreTestClass
{
	/// <summary>
	/// Gets the <see cref="BeforeAfterTestAttribute"/>s attached to the test class (and
	/// the test collection and test assembly).
	/// </summary>
	IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; }

	/// <summary>
	/// Gets the fixture factories for the class-level test fixtures on the test class.
	/// </summary>
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; }

	/// <summary>
	/// Gets the test class factory that will be used to create test class instances while tests
	/// are running.
	/// </summary>
	/// <remarks>
	/// The <see cref="FixtureMappingManager"/> that is passed to the factory will be for the class fixtures
	/// that will be used to satisfy constructor arguments for the test class.
	/// </remarks>
	Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> TestClassFactory { get; }

	/// <summary>
	/// Gets the test collection this test class belongs to.
	/// </summary>
	new ICodeGenTestCollection TestCollection { get; }
}
