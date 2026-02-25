using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility point factories related to test collections (non-AOT)

partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the given test collection.
	/// </summary>
	/// <param name="collectionDefinition">The collection definition type</param>
	/// <param name="assemblyBeforeAfterTestAttributes">The before after attributes from the test assembly,
	/// to be merged into the result.</param>
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetCollectionBeforeAfterTestAttributes(
		Type? collectionDefinition,
		IReadOnlyCollection<IBeforeAfterTestAttribute> assemblyBeforeAfterTestAttributes)
	{
		Guard.ArgumentNotNull(assemblyBeforeAfterTestAttributes);

		var warnings = new List<string>();

		try
		{
			return
				collectionDefinition is null
				? assemblyBeforeAfterTestAttributes
				: assemblyBeforeAfterTestAttributes
					.Concat(collectionDefinition.GetMatchingCustomAttributes<IBeforeAfterTestAttribute>(warnings))
					.CastOrToReadOnlyCollection();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the <see cref="ICollectionBehaviorAttribute"/> that's attached to the test assembly, if there is one.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ICollectionBehaviorAttribute? GetCollectionBehavior(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var warnings = new List<string>();

		try
		{
			var collectionBehaviorAttributes = testAssembly.GetMatchingCustomAttributes<ICollectionBehaviorAttribute>(warnings);
			return
				collectionBehaviorAttributes.Count <= 1
					? collectionBehaviorAttributes.FirstOrDefault()
					: throw new InvalidOperationException(
						string.Format(
							CultureInfo.CurrentCulture,
							"More than one assembly-level ICollectionBehaviorAttribute was discovered: {0}",
							string.Join(", ", collectionBehaviorAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
						)
					);
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the fixture types that are attached to the test collection via <see cref="IClassFixture{TFixture}"/>.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition type</param>
	/// <returns></returns>
	public static IReadOnlyCollection<Type> GetCollectionClassFixtureTypes(Type? collectionDefinition) =>
		collectionDefinition is null
			? []
			: collectionDefinition
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>))
				.Select(i => i.GenericTypeArguments[0])
				.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the fixture types that are attached to the test collection via <see cref="ICollectionFixture{TFixture}"/>.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition type</param>
	/// <returns></returns>
	public static IReadOnlyCollection<Type> GetCollectionCollectionFixtureTypes(Type? collectionDefinition) =>
		collectionDefinition is null
			? []
			: collectionDefinition
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>))
				.Select(i => i.GenericTypeArguments[0])
				.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the <see cref="CollectionDefinitionAttribute"/>s that are attached to the test assembly.
	/// Verifies that there are no collection definitions with identical names.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> GetCollectionDefinitions(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var attributeTypesByName =
			testAssembly
				.GetExportedTypes()
				.Select(type => new { Type = type, Attribute = type.GetCustomAttribute<CollectionDefinitionAttribute>() })
				.Where(list => list.Attribute is not null)
				.GroupBy(
					list => list.Attribute!.Name ?? CollectionAttribute.GetCollectionNameForType(list.Type),
					list => (list.Type, list.Attribute),
					StringComparer.OrdinalIgnoreCase
				);

		var result = new Dictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>();

		foreach (var grouping in attributeTypesByName)
		{
			var items = grouping.ToList();
			result[grouping.Key] = (items[0].Type, items[0].Attribute!);

			if (items.Count > 1)
				TestContext.Current.SendDiagnosticMessage("Multiple test collections declared with name '{0}'; chose '{1}' and ignored {2}", grouping.Key, items[0].Type.SafeName(), items.Skip(1).Select(i => i.Type).ToCommaSeparatedList());
		}

		return result;
	}

	/// <summary>
	/// Gets the traits that are attached to the test collection via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testCollectionDefinition">The test collection</param>
	/// <param name="testAssemblyTraits">The traits inherited from the test assembly</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetCollectionTraits(
		Type? testCollectionDefinition,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testAssemblyTraits)
	{
		var warnings = new List<string>();

		try
		{
			var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

			if (testAssemblyTraits is not null)
				foreach (var trait in testAssemblyTraits)
					result.AddOrGet(trait.Key).AddRange(trait.Value);

			if (testCollectionDefinition is not null)
				foreach (var traitAttribute in testCollectionDefinition.GetMatchingCustomAttributes<ITraitAttribute>(warnings))
					foreach (var kvp in traitAttribute.GetTraits())
						result.AddOrGet(kvp.Key).Add(kvp.Value);

			return result.ToReadOnly();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}
}
