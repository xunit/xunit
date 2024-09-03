using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility points related to test collections

public static partial class ExtensibilityPointFactory
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

		return
			collectionDefinition is null
				? assemblyBeforeAfterTestAttributes
				: collectionDefinition
					.GetMatchingCustomAttributes(typeof(IBeforeAfterTestAttribute))
					.Cast<IBeforeAfterTestAttribute>()
					.Concat(assemblyBeforeAfterTestAttributes)
					.CastOrToReadOnlyCollection();
	}

	/// <summary>
	/// Gets the <see cref="ICollectionBehaviorAttribute"/> that's attached to the test assembly, if there is one.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ICollectionBehaviorAttribute? GetCollectionBehavior(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var collectionBehaviorAttributes = testAssembly.GetMatchingCustomAttributes(typeof(ICollectionBehaviorAttribute));
		return
			collectionBehaviorAttributes.Count <= 1
				? collectionBehaviorAttributes.FirstOrDefault() as ICollectionBehaviorAttribute
				: throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						"More than one assembly-level ICollectionBehaviorAttribute was discovered: {0}",
						string.Join(", ", collectionBehaviorAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
					)
				);
	}

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
	/// Gets the test case orderer that's attached to a test collection. Returns <c>null</c> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestCaseOrderer? GetCollectionTestCaseOrderer(Type? collectionDefinition)
	{
		if (collectionDefinition is null)
			return null;

		var ordererAttributes = collectionDefinition.GetMatchingCustomAttributes(typeof(ITestCaseOrdererAttribute));
		if (ordererAttributes.Count > 1)
			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Found more than one test case orderer for test collection '{0}': {1}",
					collectionDefinition.SafeName(),
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				)
			);

		if (ordererAttributes.FirstOrDefault() is ITestCaseOrdererAttribute ordererAttribute)
			try
			{
				return Get<ITestCaseOrderer>(ordererAttribute.OrdererType);
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();

				TestContext.Current.SendDiagnosticMessage(
					"Collection-level test case orderer '{0}' for test collection '{1}' threw '{2}' during construction: {3}{4}{5}",
					ordererAttribute.OrdererType.SafeName(),
					collectionDefinition.SafeName(),
					innerEx.GetType().SafeName(),
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}

		return null;
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
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		if (testAssemblyTraits is not null)
			foreach (var trait in testAssemblyTraits)
				result.AddOrGet(trait.Key).AddRange(trait.Value);

		if (testCollectionDefinition is not null)
			foreach (var traitAttribute in testCollectionDefinition.GetMatchingCustomAttributes(typeof(ITraitAttribute)).Cast<ITraitAttribute>())
				foreach (var kvp in traitAttribute.GetTraits())
					result.AddOrGet(kvp.Key).Add(kvp.Value);

		return result.ToReadOnly();
	}
}
