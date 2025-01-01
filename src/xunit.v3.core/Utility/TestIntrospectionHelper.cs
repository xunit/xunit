using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Helper functions for retrieving and interpreting test and test case details from various sources
/// (like <see cref="IFactAttribute"/>, <see cref="IDataAttribute"/>, and others).
/// </summary>
public static class TestIntrospectionHelper
{
	/// <summary>
	/// Retrieve the details for a test case that is a test method decorated with an
	/// instance of <see cref="IFactAttribute"/> (or derived).
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
		Type[]? SkipExceptions,
		string? SkipReason,
		Type? SkipType,
		string? SkipUnless,
		string? SkipWhen,
		int Timeout,
		string UniqueID,
		IXunitTestMethod ResolvedTestMethod
	) GetTestCaseDetails(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute,
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

		baseDisplayName ??= factAttribute.DisplayName;
		baseDisplayName ??=
			defaultMethodDisplay == TestMethodDisplay.ClassAndMethod
				? formatter.Format(string.Format(CultureInfo.CurrentCulture, "{0}.{1}", testMethod.TestClass.TestClassName, testMethod.MethodName))
				: formatter.Format(testMethod.MethodName);

		timeout ??= factAttribute.Timeout;

		var methodGenericTypes = default(Type[]);

		if (testMethodArguments is not null)
		{
			methodGenericTypes = testMethod.ResolveGenericTypes(testMethodArguments);
			if (methodGenericTypes is not null)
				testMethod = new XunitTestMethod(testMethod.TestClass, testMethod.MakeGenericMethod(methodGenericTypes), testMethodArguments);
		}

		var testCaseDisplayName = testMethod.GetDisplayName(baseDisplayName, testMethodArguments, methodGenericTypes);
		var uniqueID = UniqueIDGenerator.ForTestCase(testMethod.UniqueID, methodGenericTypes, testMethodArguments);

		return (testCaseDisplayName, factAttribute.Explicit, factAttribute.SkipExceptions, factAttribute.Skip, factAttribute.SkipType, factAttribute.SkipUnless, factAttribute.SkipWhen, timeout.Value, uniqueID, testMethod);
	}

	/// <summary>
	/// Retrieve the details for a test case that is a test method decorated with an instance
	/// of <see cref="ITheoryAttribute"/> (or derived) when you have a data row. The data row
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
		Type[]? SkipExceptions,
		string? SkipReason,
		Type? SkipType,
		string? SkipUnless,
		string? SkipWhen,
		int Timeout,
		string UniqueID,
		IXunitTestMethod ResolvedTestMethod
	) GetTestCaseDetailsForTheoryDataRow(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute,
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
	/// Merges the traits from the test method (which already reflect the traits from the test
	/// assembly, test collection, and test class) with the traits attached to the data row.
	/// </summary>
	/// <param name="testMethod">The test method to get traits from.</param>
	/// <param name="dataRow">The data row to get traits from.</param>
	/// <returns>The traits dictionary</returns>
	public static Dictionary<string, HashSet<string>> GetTraits(
		IXunitTestMethod testMethod,
		ITheoryDataRow? dataRow)
	{
		Guard.ArgumentNotNull(testMethod);

		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var trait in testMethod.Traits)
			result.AddOrGet(trait.Key).AddRange(trait.Value);

		if (dataRow?.Traits is not null)
			foreach (var kvp in dataRow.Traits)
				result.AddOrGet(kvp.Key).AddRange(kvp.Value);

		return result;
	}

	/// <summary>
	/// Merges string-array traits (like from <see cref="IDataAttribute.Traits"/>) into an existing traits dictionary.
	/// </summary>
	/// <param name="traits">The existing traits dictionary.</param>
	/// <param name="additionalTraits">The additional traits to merge.</param>
	public static void MergeTraitsInto(
		Dictionary<string, HashSet<string>> traits,
		string[]? additionalTraits)
	{
		if (additionalTraits is null)
			return;

		var idx = 0;

		while (idx < additionalTraits.Length - 1)
		{
			traits.AddOrGet(additionalTraits[idx]).Add(additionalTraits[idx + 1]);
			idx += 2;
		}
	}
}
