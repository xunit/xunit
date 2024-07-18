using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility point factories related to test classes

public static partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the given test class.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="collectionBeforeAfterAttributes">The before after attributes from the test collection,
	/// to be merged into the result.</param>
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetClassBeforeAfterTestAttributes(
		Type testClass,
		IReadOnlyCollection<IBeforeAfterTestAttribute> collectionBeforeAfterAttributes) =>
			Guard.ArgumentNotNull(testClass)
				.GetMatchingCustomAttributes(typeof(IBeforeAfterTestAttribute))
				.Cast<IBeforeAfterTestAttribute>()
				.Concat(Guard.ArgumentNotNull(collectionBeforeAfterAttributes))
				.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the fixture types that are attached to the test class via <see cref="IClassFixture{TFixture}"/>.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="collectionClassFixtureTypes">The class fixture types from the test collection, which
	/// will be merged into the result</param>
	public static IReadOnlyCollection<Type> GetClassClassFixtureTypes(
		Type testClass,
		IReadOnlyCollection<Type> collectionClassFixtureTypes) =>
			Guard.ArgumentNotNull(testClass)
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>))
				.Select(i => i.GenericTypeArguments[0])
				.Concat(Guard.ArgumentNotNull(collectionClassFixtureTypes))
				.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the test case orderer that's attached to a test class. Returns <c>null</c> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestCaseOrderer? GetClassTestCaseOrderer(Type testClass)
	{
		Guard.ArgumentNotNull(testClass);

		var ordererAttributes = testClass.GetMatchingCustomAttributes(typeof(ITestCaseOrdererAttribute));
		if (ordererAttributes.Count > 1)
			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Found more than one test case orderer for test class '{0}': {1}",
					testClass.SafeName(),
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
					"Class-level test case orderer '{0}' for test class '{1}' threw '{2}' during construction: {3}{4}{5}",
					ordererAttribute.OrdererType,
					testClass.SafeName(),
					innerEx.GetType().SafeName(),
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}

		return null;
	}

	/// <summary>
	/// Gets the traits that are attached to the test class via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="testCollectionTraits">The traits inherited from the test collection</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetClassTraits(
		Type? testClass,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testCollectionTraits)
	{
		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		if (testCollectionTraits is not null)
			foreach (var trait in testCollectionTraits)
				result.AddOrGet(trait.Key).AddRange(trait.Value);

		if (testClass is not null)
			foreach (var traitAttribute in testClass.GetMatchingCustomAttributes(typeof(ITraitAttribute)).Cast<ITraitAttribute>())
				foreach (var kvp in traitAttribute.GetTraits())
					result.AddOrGet(kvp.Key).Add(kvp.Value);

		return result.ToReadOnly();
	}
}
