using System;
using System.Collections.Generic;
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
		IReadOnlyCollection<IBeforeAfterTestAttribute> collectionBeforeAfterAttributes)
	{
		var warnings = new List<string>();

		try
		{
			return
				Guard.ArgumentNotNull(collectionBeforeAfterAttributes)
				.Concat(Guard.ArgumentNotNull(testClass).GetMatchingCustomAttributes<IBeforeAfterTestAttribute>(warnings))
				.CastOrToReadOnlyCollection();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

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
	/// Gets the test case orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestCaseOrderer? GetClassTestCaseOrderer(Type testClass) =>
		GetClassTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(testClass, "case");

	/// <summary>
	/// Gets the test method orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestMethodOrderer? GetClassTestMethodOrderer(Type testClass) =>
		GetClassTestOrderer<ITestMethodOrderer, ITestMethodOrdererAttribute>(testClass, "method");

	static TTestOrderer? GetClassTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Type testClass,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		Guard.ArgumentNotNull(testClass);

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = testClass.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
			{
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one class-level test {0} orderer for test class '{1}': {2}",
					ordererType,
					testClass.SafeName(),
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

				return null;
			}

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					TestContext.Current.SendDiagnosticMessage(
						"Class-level test {0} orderer '{1}' for test class '{2}' threw '{3}' during construction: {4}{5}{6}",
						ordererType,
						ordererAttribute.OrdererType,
						testClass.SafeName(),
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
	/// Gets the traits that are attached to the test class via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="testCollectionTraits">The traits inherited from the test collection</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetClassTraits(
		Type? testClass,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testCollectionTraits)
	{
		var warnings = new List<string>();

		try
		{
			var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

			if (testCollectionTraits is not null)
				foreach (var trait in testCollectionTraits)
					result.AddOrGet(trait.Key).AddRange(trait.Value);

			if (testClass is not null)
				foreach (var traitAttribute in testClass.GetMatchingCustomAttributes<ITraitAttribute>(warnings))
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
