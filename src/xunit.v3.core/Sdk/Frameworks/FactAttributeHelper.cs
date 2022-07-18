using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Helper functions for retrieving and interpreting <see cref="FactAttribute"/> properties and
/// computing values for use with <see cref="TestMethodTestCase"/> and derived types.
/// </summary>
public static class FactAttributeHelper
{
	static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> assemblyTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);
	static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> typeTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);

	static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_IAssemblyInfo assembly)
	{
		Guard.ArgumentNotNull(assembly);

		return assemblyTraitAttributeCache.GetOrAdd(assembly.Name, () => assembly.GetCustomAttributes(typeof(ITraitAttribute)));
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
	/// <param name="factAttribute">The optional fact attribute that decorates the test method; if not provided, will be found via reflection.</param>
	/// <param name="traitAttributes">The optional trait attributes that decorate the test method; if not provided, will be found via reflection.</param>
	/// <param name="testMethodArguments">The optional test method arguments.</param>
	/// <param name="baseDisplayName">The optional base display name for the test method.</param>
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		string? SkipReason,
		Dictionary<string, List<string>> Traits,
		int Timeout,
		string UniqueID,
		_ITestMethod ResolvedTestMethod
	) GetTestCaseDetails(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		_IAttributeInfo? factAttribute = null,
		_IAttributeInfo[]? traitAttributes = null,
		object?[]? testMethodArguments = null,
		string? baseDisplayName = null)
	{
		Guard.ArgumentNotNull(discoveryOptions);
		Guard.ArgumentNotNull(testMethod);

		var defaultMethodDisplay = discoveryOptions.MethodDisplayOrDefault();
		var defaultMethodDisplayOptions = discoveryOptions.MethodDisplayOptionsOrDefault();
		var formatter = new DisplayNameFormatter(defaultMethodDisplay, defaultMethodDisplayOptions);

		factAttribute ??= testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).FirstOrDefault();
		if (factAttribute == null)
			throw new ArgumentException($"Could not locate the FactAttribute on test method '{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}'", nameof(testMethod));

		baseDisplayName ??= factAttribute.GetNamedArgument<string?>(nameof(FactAttribute.DisplayName));
		var factExplicit = factAttribute.GetNamedArgument<bool>(nameof(FactAttribute.Explicit));
		var factSkipReason = factAttribute.GetNamedArgument<string?>(nameof(FactAttribute.Skip));
		var factTimeout = factAttribute.GetNamedArgument<int>(nameof(FactAttribute.Timeout));

		if (baseDisplayName == null)
		{
			if (defaultMethodDisplay == TestMethodDisplay.ClassAndMethod)
				baseDisplayName = formatter.Format($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}");
			else
				baseDisplayName = formatter.Format(testMethod.Method.Name);
		}

		_ITypeInfo[]? methodGenericTypes = null;

		if (testMethodArguments != null && testMethod.Method.IsGenericMethodDefinition)
		{
			methodGenericTypes = testMethod.Method.ResolveGenericTypes(testMethodArguments);
			testMethod = new TestMethod(testMethod.TestClass, testMethod.Method.MakeGenericMethod(methodGenericTypes));
		}

		var testCaseDisplayName = testMethod.Method.GetDisplayNameWithArguments(baseDisplayName, testMethodArguments, methodGenericTypes);
		var uniqueID = UniqueIDGenerator.ForTestCase(testMethod.UniqueID, methodGenericTypes, testMethodArguments);

		traitAttributes ??= GetTraitAttributesData(testMethod);
		var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var traitAttribute in traitAttributes)
		{
			var traitDiscovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();
			if (traitDiscovererAttribute == null)
			{
				TestContext.Current?.SendDiagnosticMessage(
					"Trait attribute '{0}' on test method '{1}' does not have [TraitDiscoverer]",
					(traitAttribute as _IReflectionAttributeInfo)?.Attribute.GetType().FullName ?? "<unknown type>",
					testCaseDisplayName
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

			if (discoverer == null)
			{
				TestContext.Current?.SendDiagnosticMessage(
					"Discoverer on trait attribute '{0}' appears to be malformed (invalid type reference?)",
					(traitAttribute as _IReflectionAttributeInfo)?.Attribute.GetType().FullName ?? "<unknown type>"
				);
				continue;
			}

			foreach (var kvp in discoverer.GetTraits(traitAttribute))
				traits.Add(kvp.Key, kvp.Value);
		}

		return (testCaseDisplayName, factExplicit, factSkipReason, traits, factTimeout, uniqueID, testMethod);
	}

	/// <summary>
	/// Retrieve the details for a test case that is a test method decorated with an instance
	/// of <see cref="TheoryAttribute"/> (or derived) when you have a data row. The data row
	/// is used to augment the returned information (traits, skip reason, etc.).
	/// </summary>
	/// <param name="discoveryOptions">The options used for discovery.</param>
	/// <param name="testMethod">The test method.</param>
	/// <param name="dataRow">The data row for the test.</param>
	/// <param name="testMethodArguments">The test method arguments obtained from the <paramref name="dataRow"/> after being type-resolved.</param>
	/// <param name="theoryAttribute">The optional theory attribute that decorates the test method; if not provided, will be found via reflection.</param>
	/// <param name="traitAttributes">The optional trait attributes that decorate the test method; if not provided, will be found via reflection.</param>
	/// <param name="baseDisplayName">The optional base display name (typically from the data attribute)</param>
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		string? SkipReason,
		Dictionary<string, List<string>> Traits,
		int Timeout,
		string UniqueID,
		_ITestMethod ResolvedTestMethod
	) GetTestCaseDetails(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestMethod testMethod,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments,
		_IAttributeInfo? theoryAttribute = null,
		_IAttributeInfo[]? traitAttributes = null,
		string? baseDisplayName = null)
	{
		var result = GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute, traitAttributes, testMethodArguments, dataRow.TestDisplayName ?? baseDisplayName);

		if (dataRow.Skip != null)
			result.SkipReason = dataRow.Skip;

		if (dataRow.Explicit.HasValue)
			result.Explicit = dataRow.Explicit.Value;

		if (dataRow.Traits != null)
			foreach (var kvp in dataRow.Traits)
				result.Traits.Add(kvp.Key, kvp.Value);

		return result;
	}

	static _IAttributeInfo[] GetTraitAttributesData(_ITestMethod testMethod)
	{
		Guard.ArgumentNotNull(testMethod);

		return
			GetCachedTraitAttributes(testMethod.TestClass.Class.Assembly)
				.Concat(GetCachedTraitAttributes(testMethod.TestClass.Class))
				.Concat(testMethod.Method.GetCustomAttributes(typeof(ITraitAttribute)))
				.CastOrToArray();
	}
}
