using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Helper functions for retrieving and interpreting test and test case details from various sources
/// (like <see cref="FactAttribute"/>, <see cref="DataAttribute"/>, and others).
/// </summary>
public static class TestIntrospectionHelper
{
	static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> assemblyTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);
	static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> methodTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);
	static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> typeTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);

	static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_IAssemblyInfo assembly)
	{
		Guard.ArgumentNotNull(assembly);

		return assemblyTraitAttributeCache.GetOrAdd(assembly.Name, () => assembly.GetCustomAttributes(typeof(ITraitAttribute)));
	}

	static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_IMethodInfo method)
	{
		Guard.ArgumentNotNull(method);

		return methodTraitAttributeCache.GetOrAdd(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", method.Type.Name, method.Name), () => method.GetCustomAttributes(typeof(ITraitAttribute)));
	}

	static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_ITypeInfo type)
	{
		Guard.ArgumentNotNull(type);

		return typeTraitAttributeCache.GetOrAdd(type.Name, () => type.GetCustomAttributes(typeof(ITraitAttribute)));
	}

	/// <summary>
	/// Retrieve the details for a test case that is a test method decorated with an
	/// instance of <see cref="FactAttribute"/> (or derived).
	/// </summary>
	/// <param name="discoveryOptions">The options used for discovery.</param>
	/// <param name="testMethod">The test method.</param>
	/// <param name="factAttribute">The fact attribute that decorates the test method.</param>
	/// <param name="testMethodArguments">The optional test method arguments.</param>
	/// <param name="timeout">The optional timeout; if not provided, will be looked up from the <paramref name="factAttribute"/>.</param>
	/// <param name="baseDisplayName">The optional base display name for the test method.</param>
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		string? SkipReason,
		int Timeout,
		string UniqueID,
		_ITestMethod ResolvedTestMethod
	) GetTestCaseDetails(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo factAttribute,
		object?[]? testMethodArguments = null,
		int? timeout = null,
		string? baseDisplayName = null)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(factAttribute);

		var defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault();
		var defaultMethodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
		var formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);

		baseDisplayName ??= factAttribute.GetNamedArgument<string?>(nameof(FactAttribute.DisplayName));
		var factExplicit = factAttribute.GetNamedArgument<bool>(nameof(FactAttribute.Explicit));
		var factSkipReason = factAttribute.GetNamedArgument<string?>(nameof(FactAttribute.Skip));
		timeout ??= factAttribute.GetNamedArgument<int>(nameof(FactAttribute.Timeout));

		if (baseDisplayName is null)
		{
			if (defaultMethodDisplay == TestMethodDisplay.ClassAndMethod)
				baseDisplayName = formatter.Format(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", testMethod.TestClass.Class.Name, testMethod.Method.Name));
			else
				baseDisplayName = formatter.Format(testMethod.Method.Name);
		}

		_ITypeInfo[]? methodGenericTypes = null;

		if (testMethodArguments is not null && testMethod.Method.IsGenericMethodDefinition)
		{
			methodGenericTypes = testMethod.Method.ResolveGenericTypes(testMethodArguments);
			testMethod = new TestMethod(testMethod.TestClass, testMethod.Method.MakeGenericMethod(methodGenericTypes));
		}

		var testCaseDisplayName = testMethod.Method.GetDisplayNameWithArguments(baseDisplayName, testMethodArguments, methodGenericTypes);
		var uniqueID = UniqueIDGenerator.ForTestCase(testMethod.UniqueID, methodGenericTypes, testMethodArguments);

		return (testCaseDisplayName, factExplicit, factSkipReason, timeout.Value, uniqueID, testMethod);
	}

	/// <summary>
	/// Retrieve the details for a test case that is a test method decorated with an instance
	/// of <see cref="TheoryAttribute"/> (or derived) when you have a data row. The data row
	/// is used to augment the returned information (traits, skip reason, etc.).
	/// </summary>
	/// <param name="discoveryOptions">The options used for discovery.</param>
	/// <param name="testMethod">The test method.</param>
	/// <param name="theoryAttribute">The theory attribute that decorates the test method.</param>
	/// <param name="dataRow">The data row for the test.</param>
	/// <param name="testMethodArguments">The test method arguments obtained from the <paramref name="dataRow"/> after being type-resolved.</param>
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		string? SkipReason,
		int Timeout,
		string UniqueID,
		_ITestMethod ResolvedTestMethod
	) GetTestCaseDetailsForTheoryDataRow(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(theoryAttribute);
		Guard.ArgumentNotNull(dataRow);
		Guard.ArgumentNotNull(testMethodArguments);

		var result = GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute, testMethodArguments, dataRow.Timeout, dataRow.TestDisplayName);

		if (dataRow.Explicit.HasValue)
			result.Explicit = dataRow.Explicit.Value;

		if (dataRow.Skip is not null)
			result.SkipReason = dataRow.Skip;

		return result;
	}

	/// <summary>
	/// Retrieve the traits for a test method (merging in the traits from the optional data row, which is
	/// assumed to already have traits that were merged from the data row itself and the data attribute).
	/// </summary>
	/// <param name="testMethod">The test method to get traits from.</param>
	/// <param name="dataRow">The data row to get traits from.</param>
	/// <returns>The traits dictionary</returns>
	public static Dictionary<string, List<string>> GetTraits(
		_ITestMethod testMethod,
		ITheoryDataRow? dataRow = null)
	{
		Guard.ArgumentNotNull(testMethod);

		var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		// Traits from the test assembly, test class, and test method
		var traitAttributes =
			GetCachedTraitAttributes(testMethod.TestClass.Class.Assembly)
				.Concat(GetCachedTraitAttributes(testMethod.TestClass.Class))
				.Concat(GetCachedTraitAttributes(testMethod.Method));

		foreach (var traitAttribute in traitAttributes)
		{
			var traitDiscovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();
			if (traitDiscovererAttribute is null)
			{
				TestContext.Current?.SendDiagnosticMessage(
					"Trait attribute '{0}' on test method '{1}.{2}' does not have [TraitDiscoverer]",
					(traitAttribute as _IReflectionAttributeInfo)?.Attribute.GetType().FullName ?? "<unknown type>",
					testMethod.TestClass.Class.Name,
					testMethod.Method.Name
				);
				continue;
			}

			var discoverer = default(ITraitDiscoverer);

			try
			{
				discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(traitDiscovererAttribute);
			}
			catch (MissingMemberException)
			{
				// This already logs a diagnostic message about the mismatched constructor, so just move on
				continue;
			}

			if (discoverer is null)
			{
				TestContext.Current?.SendDiagnosticMessage(
					"Discoverer on trait attribute '{0}' appears to be malformed (invalid type reference?)",
					(traitAttribute as _IReflectionAttributeInfo)?.Attribute.GetType().FullName ?? "<unknown type>"
				);
				continue;
			}

			foreach (var kvp in discoverer.GetTraits(traitAttribute))
				result.GetOrAdd(kvp.Key).Add(kvp.Value);
		}

		// Traits from the data row
		if (dataRow?.Traits is not null)
			foreach (var kvp in dataRow.Traits)
				result.GetOrAdd(kvp.Key).AddRange(kvp.Value);

		return result;
	}

	/// <summary>
	/// Merges string-array traits (like from <see cref="DataAttribute"/>) into an existing traits dictionary.
	/// </summary>
	/// <param name="traits">The existing traits dictionary.</param>
	/// <param name="additionalTraits">The additional traits to merge.</param>
	public static void MergeTraitsInto(
		Dictionary<string, List<string>> traits,
		string[]? additionalTraits)
	{
		if (additionalTraits is null)
			return;

		var idx = 0;

		while (idx < additionalTraits.Length - 1)
		{
			traits.GetOrAdd(additionalTraits[idx]).Add(additionalTraits[idx + 1]);
			idx += 2;
		}
	}
}
