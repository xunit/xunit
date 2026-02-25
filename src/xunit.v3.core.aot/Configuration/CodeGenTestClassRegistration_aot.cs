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
	public IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? ClassFixtureFactories { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the class-level test case orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestCaseOrdererType { get; set; }
#else
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }
#endif

	/// <summary>
	/// Gets the factory for the class-level test method orderer.
	/// </summary>
#if XUNIT_GENERATOR
	public required string? TestMethodOrdererType { get; set; }
#else
	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; init; }
#endif

#if !XUNIT_GENERATOR

	static readonly Dictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> emptyFixtureFactories = [];
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
				var traitAttributes = Class.GetCustomAttributes<TraitAttribute>().CastOrToReadOnlyCollection();

				if (traitAttributes.Count == 0)
					traits = testCollection.Traits;
				else
				{
					var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
					foreach (var kvp in testCollection.Traits)
						foreach (var value in kvp.Value)
							newTraits.AddOrGet(kvp.Key).Add(value);

					foreach (var traitAttribute in traitAttributes)
						newTraits.AddOrGet(traitAttribute.Name).Add(traitAttribute.Value);

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
		if (TestCaseOrdererType is not null)
			initValues.Add($"TestCaseOrdererFactory = () => new {TestCaseOrdererType}()");
		if (TestMethodOrdererType is not null)
			initValues.Add($"TestMethodOrdererFactory = () => new {TestMethodOrdererType}()");

		return $"new global::Xunit.v3.CodeGenTestClassRegistration() {{ {string.Join(", ", initValues)} }}";
	}

#endif  // XUNIT_GENERATOR
}
