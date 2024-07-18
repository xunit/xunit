using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility point factories related to test assemblies

public static partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the given test assembly.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetAssemblyBeforeAfterTestAttributes(Assembly testAssembly) =>
		Guard.ArgumentNotNull(testAssembly)
			.GetMatchingCustomAttributes(typeof(IBeforeAfterTestAttribute))
			.Cast<IBeforeAfterTestAttribute>()
			.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the fixture types that are attached to the test assembly via <see cref="IAssemblyFixtureAttribute"/>s.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyCollection<Type> GetAssemblyFixtureTypes(Assembly testAssembly) =>
		Guard.ArgumentNotNull(testAssembly)
			.GetMatchingCustomAttributes(typeof(IAssemblyFixtureAttribute))
			.OfType<IAssemblyFixtureAttribute>()
			.Select(a => a.AssemblyFixtureType)
			.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the test case orderer that's attached to a test assembly. Returns <c>null</c> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCaseOrderer? GetAssemblyTestCaseOrderer(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var ordererAttributes = testAssembly.GetMatchingCustomAttributes(typeof(ITestCaseOrdererAttribute));
		if (ordererAttributes.Count > 1)
			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Found more than one test case orderer for test assembly: {0}",
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
					"Assembly-level test case orderer '{0}' threw '{1}' during construction: {2}{3}{4}",
					ordererAttribute.OrdererType.SafeName(),
					innerEx.GetType().SafeName(),
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}

		return null;
	}

	/// <summary>
	/// Gets the test collection orderer that's attached to a test assembly. Returns <c>null</c> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCollectionOrderer? GetAssemblyTestCollectionOrderer(Assembly testAssembly)
	{
		var ordererAttributes = testAssembly.GetMatchingCustomAttributes(typeof(ITestCollectionOrdererAttribute));
		if (ordererAttributes.Count > 1)
			throw new InvalidOperationException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Found more than one test collection orderer for test assembly: {0}",
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				)
			);

		if (ordererAttributes.FirstOrDefault() is ITestCollectionOrdererAttribute ordererAttribute)
			try
			{
				return Get<ITestCollectionOrderer>(ordererAttribute.OrdererType);
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();

				TestContext.Current.SendDiagnosticMessage(
					"Assembly-level test collection orderer '{0}' threw '{1}' during construction: {2}{3}{4}",
					ordererAttribute.OrdererType.SafeName(),
					innerEx.GetType().SafeName(),
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}

		return null;
	}

	/// <summary>
	/// Gets the traits that are attached to the test assembly via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetAssemblyTraits(Assembly testAssembly)
	{
		Guard.ArgumentNotNull(testAssembly);

		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var traitAttribute in testAssembly.GetMatchingCustomAttributes(typeof(ITraitAttribute)).Cast<ITraitAttribute>())
			foreach (var kvp in traitAttribute.GetTraits())
				result.AddOrGet(kvp.Key).Add(kvp.Value);

		return result.ToReadOnly();
	}
}
