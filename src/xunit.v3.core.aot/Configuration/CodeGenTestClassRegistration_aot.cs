using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// Contains information about a test class, as discovered via code generation.
/// </summary>
public class CodeGenTestClassRegistration
{
	static readonly Dictionary<Type, FixtureFactory> emptyFixtureFactories = [];
	readonly Lock factoryLock = new();
	ICodeGenTestClass? testClass;

	/// <summary>
	/// Gets the type of the test class.
	/// </summary>
	public required Type Class { get; set; }

	/// <summary>
	/// Gets the factory for the test class.
	/// </summary>
	public Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> ClassFactory { get; init; } =
		_ => new(new CoreTestClassCreationResult(null));

	/// <summary>
	/// Gets the class fixtures associated with the test class.
	/// </summary>
	public IReadOnlyDictionary<Type, FixtureFactory>? ClassFixtureFactories { get; init; }

	/// <summary>
	/// Gets the factory for the class-level test case orderer.
	/// </summary>
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }

	/// <summary>
	/// Gets the factory for the class-level test method orderer.
	/// </summary>
	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; init; }

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
}
