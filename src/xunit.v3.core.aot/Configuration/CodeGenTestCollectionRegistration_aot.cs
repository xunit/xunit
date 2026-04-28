#if XUNIT_GENERATOR
namespace Xunit.Generators;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Contains information about a test collection, as discovered via code generation.
/// </summary>
public sealed class CodeGenTestCollectionRegistration
#if XUNIT_GENERATOR
	: IEquatable<CodeGenTestCollectionRegistration?>
#endif
{
	/// <summary>
	/// Gets the class fixtures associated with the test collection.
	/// </summary>
#if XUNIT_GENERATOR
	public required IReadOnlyCollection<(string Type, string Factory)> ClassFixtures { get; set; }
#else
	public IReadOnlyDictionary<Type, FixtureFactory>? ClassFixtureFactories { get; init; }
#endif

	/// <summary>
	/// Gets the collection fixtures associated with the test collection.
	/// </summary>
#if XUNIT_GENERATOR
	public required IReadOnlyCollection<(string Type, string Factory)> CollectionFixtures { get; set; }
#else
	public IReadOnlyDictionary<Type, FixtureFactory>? CollectionFixtureFactories { get; init; }
#endif

	/// <summary>
	/// A flag indicating whether this collection wants to run without being parallelized against
	/// other test collections.
	/// </summary>
#if XUNIT_GENERATOR
	public required bool DisableParallelization { get; set; }
#else
	public bool DisableParallelization { get; init; }
#endif

#if !XUNIT_GENERATOR

	/// <summary>
	/// Gets the empty test collection registration.
	/// </summary>
	public static CodeGenTestCollectionRegistration Empty { get; } = new();

#endif  // !XUNIT_GENERATOR

	/// <summary>
	/// Gets the factory for the collection-level test case orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestCaseOrdererFactory { get; set; }
#else
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the collection-level test class orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestClassOrdererFactory { get; set; }
#else
	public Func<ITestClassOrderer>? TestClassOrdererFactory { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the collection-level test method orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestMethodOrdererFactory { get; set; }
#else
	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; init; }
#endif

#if XUNIT_GENERATOR
	/// <summary>
	/// The traits attached to the test collection
	/// </summary>
	public required IReadOnlyDictionary<string, HashSet<string>>? Traits { get; set; }
#endif

	/// <summary>
	/// Gets the type associated with the collection definition.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? Type { get; set; }
#else
	public Type? Type { get; init; }
#endif

#if XUNIT_GENERATOR

	public override bool Equals(object? obj) =>
		Equals(obj as CodeGenTestCollectionRegistration);

	public bool Equals(CodeGenTestCollectionRegistration? other) =>
		other is not null &&
		ComparerHelper.Equals(ClassFixtures, other.ClassFixtures) &&
		ComparerHelper.Equals(CollectionFixtures, other.CollectionFixtures) &&
		ComparerHelper.Equals(DisableParallelization, other.DisableParallelization) &&
		ComparerHelper.Equals(TestCaseOrdererFactory, other.TestCaseOrdererFactory) &&
		ComparerHelper.Equals(TestClassOrdererFactory, other.TestClassOrdererFactory) &&
		ComparerHelper.Equals(TestMethodOrdererFactory, other.TestMethodOrdererFactory) &&
		ComparerHelper.Equals(Traits, other.Traits) &&
		ComparerHelper.Equals(Type, other.Type);

	public override int GetHashCode() =>
		Hasher.Start()
			.With(ClassFixtures)
			.With(CollectionFixtures)
			.With(DisableParallelization)
			.With(TestCaseOrdererFactory)
			.With(TestClassOrdererFactory)
			.With(TestMethodOrdererFactory)
			.With(Traits)
			.With(Type);

	/// <summary>
	/// Gets init values used by the source generator.
	/// </summary>
	public string ToGeneratedInit()
	{
		var initValues = new List<string>();

		if (ClassFixtures.Count != 0)
			initValues.Add($"ClassFixtureFactories = {CodeGenRegistration.ToFixtureFactories(ClassFixtures)}");
		if (CollectionFixtures.Count != 0)
			initValues.Add($"CollectionFixtureFactories = {CodeGenRegistration.ToFixtureFactories(CollectionFixtures)}");
		if (DisableParallelization)
			initValues.Add("DisableParallelization = true");
		if (TestCaseOrdererFactory is not null)
			initValues.Add($"TestCaseOrdererFactory = () => {TestCaseOrdererFactory}");
		if (TestClassOrdererFactory is not null)
			initValues.Add($"TestClassOrdererFactory = () => {TestClassOrdererFactory}");
		if (TestMethodOrdererFactory is not null)
			initValues.Add($"TestMethodOrdererFactory = () => {TestMethodOrdererFactory}");
		if (Type is not null)
			initValues.Add($"Type = typeof({Type})");

		if (initValues.Count == 0)
			return "global::Xunit.v3.CodeGenTestCollectionRegistration.Empty";

		return $"new global::Xunit.v3.CodeGenTestCollectionRegistration() {{ {string.Join(", ", initValues)} }}";
	}

#endif  // XUNIT_GENERATOR
}
