#if !XUNIT_GENERATOR
using System.Reflection;
#endif

#if XUNIT_GENERATOR
namespace Xunit.Generators;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Contains information about a test class, as discovered via code generation.
/// </summary>
public class CodeGenTestClassRegistration
#if XUNIT_GENERATOR
	: IEquatable<CodeGenTestClassRegistration?>
#endif
{
	/// <summary>
	/// Gets the type of the test class.
	/// </summary>
#if XUNIT_GENERATOR
	public required string Class { get; set; }
#else
	public required Type Class { get; set; }
#endif

	/// <summary>
	/// Gets the factory for the test class.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? ClassFactory { get; set; }
#else
	public Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> ClassFactory { get; init; } =
		_ => new(new CoreTestClassCreationResult(null));
#endif

	/// <summary>
	/// Gets the class fixtures associated with the test class.
	/// </summary>
#if XUNIT_GENERATOR
	public required IReadOnlyCollection<(string Type, string Factory)> ClassFixtures { get; set; }
#else
	public IReadOnlyDictionary<Type, FixtureFactory>? ClassFixtureFactories { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the class-level test case orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestCaseOrdererFactory { get; set; }
#else
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the class-level test method orderer.
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

#if XUNIT_GENERATOR

	public override bool Equals(object? obj) =>
		Equals(obj as CodeGenTestClassRegistration);

	public bool Equals(CodeGenTestClassRegistration? other) =>
		other is not null &&
		ComparerHelper.Equals(Class, other.Class) &&
		ComparerHelper.Equals(ClassFactory, other.ClassFactory) &&
		ComparerHelper.Equals(ClassFixtures, other.ClassFixtures) &&
		ComparerHelper.Equals(TestCaseOrdererFactory, other.TestCaseOrdererFactory) &&
		ComparerHelper.Equals(TestMethodOrdererFactory, other.TestMethodOrdererFactory) &&
		ComparerHelper.Equals(Traits, other.Traits);

	public override int GetHashCode() =>
		Hasher.Start().With(Class).With(ClassFactory).With(ClassFixtures).With(TestCaseOrdererFactory).With(TestMethodOrdererFactory).With(Traits);

#endif  // XUNIT_GENERATOR

#if !XUNIT_GENERATOR

	static readonly Dictionary<Type, FixtureFactory> emptyFixtureFactories = [];
	readonly object factoryLock = new();
	ICodeGenTestClass? testClass;

	/// <summary>
	/// Generates the <see cref="ICodeGenTestClass"/> instance for this registration.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public ICodeGenTestClass GetTestClass(ICodeGenTestAssembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		lock (factoryLock)
			if (testClass is null)
			{
				var testCollection = testAssembly.TestCollectionFactory.Get(Class);

				IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;
				var testClassTraits = RegisteredEngineConfig.GetTestClassTraits(Class);

				if (testClassTraits is null || testClassTraits.Count == 0)
					traits = testCollection.Traits;
				else
				{
					var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

					foreach (var kvp in testCollection.Traits)
						newTraits.AddOrGet(kvp.Key).AddRange(kvp.Value);

					foreach (var kvp in testClassTraits)
						newTraits.AddOrGet(kvp.Key).AddRange(kvp.Value);

					traits = newTraits.ToReadOnly();
				}

				var beforeAfterTestAttributes =
					testCollection
						.BeforeAfterTestAttributes
						.Concat(Class.GetCustomAttributes<BeforeAfterTestAttribute>());

				testClass = new CodeGenTestClass(
					beforeAfterTestAttributes.CastOrToReadOnlyCollection(),
					Class,
					ClassFixtureFactories ?? emptyFixtureFactories,
					ClassFactory,
					testCollection,
					traits
				);
			}

		return testClass;
	}

#endif  // !XUNIT_GENERATOR

#if XUNIT_GENERATOR

	/// <summary>
	/// Gets init values used by the source generator.
	/// </summary>
	public string ToGeneratedInit()
	{
		var initValues = new List<string>()
		{
			$"Class = typeof({Class})",
		};

		if (ClassFactory is not null)
			initValues.Add($"ClassFactory = {ClassFactory}");
		if (ClassFixtures.Count != 0)
			initValues.Add($"ClassFixtureFactories = {CodeGenRegistration.ToFixtureFactories(ClassFixtures)}");
		if (TestCaseOrdererFactory is not null)
			initValues.Add($"TestCaseOrdererFactory = () => {TestCaseOrdererFactory}");
		if (TestMethodOrdererFactory is not null)
			initValues.Add($"TestMethodOrdererFactory = () => {TestMethodOrdererFactory}");

		return $"new global::Xunit.v3.CodeGenTestClassRegistration() {{ {string.Join(", ", initValues)} }}";
	}

#endif  // XUNIT_GENERATOR
}
