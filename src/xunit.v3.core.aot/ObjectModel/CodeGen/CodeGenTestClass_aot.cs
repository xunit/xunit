using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ICodeGenTestClass"/> for xUnit.net v3 tests.
/// </summary>
/// <param name="beforeAfterTestAttributes">The <see cref="BeforeAfterTestAttribute"/>s attached to the test class</param>
/// <param name="class">The test class type</param>
/// <param name="classFixtureFactories">The fixture factories for class-level test fixtures (on the class)</param>
/// <param name="testClassFactory">The test class factory that was registered during code generation</param>
/// <param name="testCollection">The test collection this test class belongs to</param>
/// <param name="traits">The class-level traits</param>
/// <param name="uniqueID">The optional test class unique ID</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public sealed class CodeGenTestClass(
	IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes,
	Type @class,
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> classFixtureFactories,
	Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> testClassFactory,
	ICodeGenTestCollection testCollection,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string? uniqueID = null) :
		ICodeGenTestClass
{
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer = new(() => RegisteredEngineConfig.GetClassTestCaseOrderer(@class));
	readonly Lazy<ITestMethodOrderer?> testMethodOrderer = new(() => RegisteredEngineConfig.GetClassTestMethodOrderer(@class));

	/// <inheritdoc/>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; } =
		Guard.ArgumentNotNull(beforeAfterTestAttributes);

	/// <inheritdoc/>
	public Type Class { get; } =
		Guard.ArgumentNotNull(@class);

	/// <inheritdoc/>
	public IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; } =
		Guard.ArgumentNotNull(classFixtureFactories);

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> TestClassFactory { get; } =
		Guard.ArgumentNotNull(testClassFactory);

	/// <inheritdoc/>
	public string TestClassName =>
		Class.SafeName();

	/// <inheritdoc/>
	public string? TestClassNamespace =>
		Class.Namespace;

	/// <inheritdoc/>
	public string TestClassSimpleName =>
		Class.ToSimpleName();

	/// <inheritdoc/>
	public ICodeGenTestCollection TestCollection { get; } =
		Guard.ArgumentNotNull(testCollection);

	ICoreTestCollection ICoreTestClass.TestCollection => TestCollection;

	ITestCollection ITestClass.TestCollection => TestCollection;

	/// <inheritdoc/>
	public ITestMethodOrderer? TestMethodOrderer =>
		testMethodOrderer.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
		Guard.ArgumentNotNull(traits);

	/// <inheritdoc/>
	public string UniqueID { get; } =
		uniqueID ?? UniqueIDGenerator.ForTestClass(testCollection.UniqueID, @class.SafeName());
}
