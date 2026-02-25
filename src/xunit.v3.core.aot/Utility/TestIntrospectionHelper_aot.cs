using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Helper functions for retrieving and interpreting test and test case details from various sources
/// (like <see cref="FactAttribute"/>, <see cref="DataAttribute"/>, and others).
/// </summary>
partial class TestIntrospectionHelper
{
	/// <summary>
	/// Test introspection is performed by the source generators in Native AOT
	/// </summary>
	[Obsolete("Test introspection is performed by the source generators in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		Type[]? SkipExceptions,
		string? SkipReason,
		Type? SkipType,
		string? SkipUnless,
		string? SkipWhen,
		string? SourceFilePath,
		int? SourceLineNumber,
		int Timeout,
		string UniqueID,
		IXunitTestMethod ResolvedTestMethod
	) GetTestCaseDetails(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		IFactAttribute factAttribute,
		object?[]? testMethodArguments = null,
		int? timeout = null,
		string? baseDisplayName = null,
		string? label = null) =>
			throw new NotSupportedException("Test introspection is performed by the source generators in Native AOT");

	/// <summary>
	/// Test introspection is performed by the source generators in Native AOT
	/// </summary>
	[Obsolete("Test introspection is performed by the source generators in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static (
		string TestCaseDisplayName,
		bool Explicit,
		Type[]? SkipExceptions,
		string? SkipReason,
		Type? SkipType,
		string? SkipUnless,
		string? SkipWhen,
		string? SourceFilePath,
		int? SourceLineNumber,
		int Timeout,
		string UniqueID,
		IXunitTestMethod ResolvedTestMethod
	) GetTestCaseDetailsForTheoryDataRow(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		IXunitTestMethod testMethod,
		ITheoryAttribute theoryAttribute,
		ITheoryDataRow dataRow,
		object?[] testMethodArguments) =>
			throw new NotSupportedException("Test introspection is performed by the source generators in Native AOT");

	/// <summary>
	/// Test introspection is performed by the source generators in Native AOT
	/// </summary>
	[Obsolete("Test introspection is performed by the source generators in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Dictionary<string, HashSet<string>> GetTraits(
		IXunitTestMethod testMethod,
		ITheoryDataRow? dataRow) =>
			throw new NotSupportedException("Test introspection is performed by the source generators in Native AOT");
}
