using Xunit.Sdk;
namespace Xunit.v3;

/// <summary>
/// Contains information about a test collection, as discovered via code generation.
/// </summary>
public sealed class CodeGenTestCollectionRegistration
{
	/// <summary>
	/// Gets the class fixtures associated with the test collection.
	/// </summary>
	public IReadOnlyDictionary<Type, FixtureFactory>? ClassFixtureFactories { get; init; }

	/// <summary>
	/// Gets the collection fixtures associated with the test collection.
	/// </summary>
	public IReadOnlyDictionary<Type, FixtureFactory>? CollectionFixtureFactories { get; init; }

	/// <summary>
	/// A flag indicating whether this collection wants to run without being parallelized against
	/// other test collections.
	/// </summary>
	public bool DisableParallelization
	{
		get => ParallelismOptions?.HasFlag(Sdk.ParallelismOptions.Collections) == false;
		init
		{
			if (value)
			{
				if (ParallelismOptions.HasValue)
				{
					ParallelismOptions &= ~Sdk.ParallelismOptions.Collections;
				}
				else
				{
					ParallelismOptions = Sdk.ParallelismOptions.None;
				}
			}
		}
	}

	/// <summary>
	/// Options which determine the amount of parallelization to allow for this test collection.
	/// </summary>
	public ParallelismOptions? ParallelismOptions { get; init; }

	/// <summary>
	/// Gets the empty test collection registration.
	/// </summary>
	public static CodeGenTestCollectionRegistration Empty { get; } = new();

	/// <summary>
	/// Gets the factory for the collection-level test case orderer.
	/// </summary>
	public Func<ITestCaseOrderer>? TestCaseOrdererFactory { get; init; }

	/// <summary>
	/// Gets the factory for the collection-level test class orderer.
	/// </summary>
	public Func<ITestClassOrderer>? TestClassOrdererFactory { get; init; }

	/// <summary>
	/// Gets the factory for the collection-level test method orderer.
	/// </summary>
	public Func<ITestMethodOrderer>? TestMethodOrdererFactory { get; init; }

	/// <summary>
	/// Gets the type associated with the collection definition.
	/// </summary>
	public Type? Type { get; init; }
}
