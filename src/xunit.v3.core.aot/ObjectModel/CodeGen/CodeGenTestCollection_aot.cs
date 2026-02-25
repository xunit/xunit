using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="ICodeGenTestCollection"/> for xUnit.net v3 tests.
/// </summary>
/// <param name="beforeAfterTestAttributes">The <see cref="BeforeAfterTestAttribute"/>s attached to this test collection</param>
/// <param name="classFixtureFactories">The fixture factories for class-level test fixtures (on the collection)</param>
/// <param name="collectionFixtureFactories">The fixture factories for collection-level test fixtures (on the collection)</param>
/// <param name="disableParallelization">Determines whether tests in this collection run in parallel with any other collections</param>
/// <param name="testAssembly">The test assembly this collection belongs to</param>
/// <param name="testCollectionClass">The optional type that contains the test collection definition</param>
/// <param name="testCollectionDisplayName">The display name of the test collection</param>
/// <param name="traits">The collection-level traits</param>
/// <param name="uniqueID">The optional test collection unique ID</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public sealed class CodeGenTestCollection(
	IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes,
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> classFixtureFactories,
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> collectionFixtureFactories,
	bool disableParallelization,
	ICodeGenTestAssembly testAssembly,
	Type? testCollectionClass,
	string testCollectionDisplayName,
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> traits,
	string? uniqueID = null) :
		ICodeGenTestCollection
{
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer = new(() => RegisteredEngineConfig.GetCollectionTestCaseOrderer(testCollectionClass));
	readonly Lazy<ITestClassOrderer?> testClassOrderer = new(() => RegisteredEngineConfig.GetCollectionTestClassOrderer(testCollectionClass));
	readonly Lazy<ITestMethodOrderer?> testMethodOrderer = new(() => RegisteredEngineConfig.GetCollectionTestMethodOrderer(testCollectionClass));

	/// <inheritdoc/>
	public IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; } =
		Guard.ArgumentNotNull(beforeAfterTestAttributes);

	/// <inheritdoc/>
	public IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; } =
		Guard.ArgumentNotNull(classFixtureFactories);

	/// <inheritdoc/>
	public IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> CollectionFixtureFactories { get; } =
		Guard.ArgumentNotNull(collectionFixtureFactories);

	/// <inheritdoc/>
	public bool DisableParallelization =>
		disableParallelization;

	/// <inheritdoc/>
	public ICodeGenTestAssembly TestAssembly { get; } =
		Guard.ArgumentNotNull(testAssembly);

	ICoreTestAssembly ICoreTestCollection.TestAssembly => TestAssembly;

	ITestAssembly ITestCollection.TestAssembly => TestAssembly;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public ITestClassOrderer? TestClassOrderer =>
		testClassOrderer.Value;

	/// <inheritdoc/>
	public string? TestCollectionClassName =>
		testCollectionClass?.FullName;

	/// <inheritdoc/>
	public string TestCollectionDisplayName { get; } =
		Guard.ArgumentNotNull(testCollectionDisplayName);

	/// <inheritdoc/>
	public ITestMethodOrderer? TestMethodOrderer =>
		testMethodOrderer.Value;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; } =
		Guard.ArgumentNotNull(traits);

	/// <inheritdoc/>
	public string UniqueID { get; } =
		uniqueID ?? UniqueIDGenerator.ForTestCollection(testAssembly.UniqueID, testCollectionDisplayName, testCollectionClass?.FullName);
}
