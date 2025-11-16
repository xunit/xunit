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
	/// Gets the test case orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestCaseOrderer? GetCollectionTestCaseOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(collectionDefinition, "case");

	/// <summary>
	/// Gets the test class orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestClassOrderer? GetCollectionTestClassOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestClassOrderer, ITestClassOrdererAttribute>(collectionDefinition, "class");

	/// <summary>
	/// Gets the test method orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestMethodOrderer? GetCollectionTestMethodOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestMethodOrderer, ITestMethodOrdererAttribute>(collectionDefinition, "method");

	static TTestOrderer? GetCollectionTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Type? collectionDefinition,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		if (collectionDefinition is null)
			return null;

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = collectionDefinition.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one collection-level test {0} orderer for test collection '{1}': {2}",
					ordererType,
					collectionDefinition.SafeName(),
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					TestContext.Current.SendDiagnosticMessage(
						"Collection-level test {0} orderer '{1}' for test collection '{2}' threw '{3}' during construction: {4}{5}{6}",
						ordererType,
						ordererAttribute.OrdererType,
						collectionDefinition.SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message ?? "(null message)",
						Environment.NewLine,
						innerEx.StackTrace
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
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
