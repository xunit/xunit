using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base class with common functionality between <see cref="CollectionPerAssemblyTestCollectionFactory"/>
/// and <see cref="CollectionPerClassTestCollectionFactory"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
public abstract class TestCollectionFactoryBase(ICodeGenTestAssembly testAssembly) :
	ICodeGenTestCollectionFactory
{
	readonly ConcurrentDictionary<string, ICodeGenTestCollection> testCollections = new();

	/// <inheritdoc/>
	public abstract string DisplayName { get; }

	/// <summary>
	/// Gets the test assembly.
	/// </summary>
	protected ICodeGenTestAssembly TestAssembly { get; } =
		Guard.ArgumentNotNull(testAssembly);

	ICodeGenTestCollection CreateCollection(CollectionAttributeBase attribute)
	{
		if (!TestAssembly.CollectionDefinitions.TryGetValue(attribute.Name, out var definition))
			definition = CodeGenTestCollectionRegistration.Empty;

		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits;
		var traitAttributes = definition.Type?.GetCustomAttributes<TraitAttribute>().CastOrToReadOnlyCollection();

		if (traitAttributes is null || traitAttributes.Count == 0)
			traits = testAssembly.Traits;
		else
		{
			var newTraits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (var kvp in testAssembly.Traits)
				foreach (var value in kvp.Value)
					newTraits.AddOrGet(kvp.Key).Add(value);

			foreach (var traitAttribute in traitAttributes)
				newTraits.AddOrGet(traitAttribute.Name).Add(traitAttribute.Value);

			traits = newTraits.ToReadOnly();
		}

		IEnumerable<BeforeAfterTestAttribute> beforeAfterTestAttributes = TestAssembly.BeforeAfterTestAttributes;
		if (definition.Type is not null)
			beforeAfterTestAttributes = beforeAfterTestAttributes.Concat(definition.Type.GetCustomAttributes<BeforeAfterTestAttribute>());

		return new CodeGenTestCollection(
			beforeAfterTestAttributes.CastOrToReadOnlyCollection(),
			definition.ClassFixtureFactories ?? CodeGenHelper.EmptyFixtureFactories,
			definition.CollectionFixtureFactories ?? CodeGenHelper.EmptyFixtureFactories,
			definition.DisableParallelization,
			TestAssembly,
			testCollectionClass: definition.Type,
			attribute.Name,
			traits
		);
	}

	/// <inheritdoc/>
	public ICodeGenTestCollection Get(Type testClass)
	{
		Guard.ArgumentNotNull(testClass);

		var attributes = testClass.GetCustomAttributes<CollectionAttributeBase>().CastOrToReadOnlyCollection();

		return attributes.Count > 1
			? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "More than one collection attribute was found on test class {0}: {1}", testClass.SafeName(), string.Join(", ", attributes.Select(a => a.GetType()).ToCommaSeparatedList())))
			: attributes.FirstOrDefault() is CollectionAttributeBase attribute
				? testCollections.GetOrAdd(attribute.Name, _ => CreateCollection(attribute))
				: testCollections.GetOrAdd(CollectionAttribute.GetCollectionNameForType(testClass), _ => GetDefaultTestCollection(testClass));
	}

	/// <summary>
	/// Override to provide a test collection when the given test class is not decorated
	/// with any test collection attributes.
	/// </summary>
	/// <param name="testClass">The test class</param>
	protected abstract ICodeGenTestCollection GetDefaultTestCollection(Type testClass);
}
