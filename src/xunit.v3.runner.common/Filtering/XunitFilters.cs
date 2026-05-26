using Microsoft.VisualStudio.TestPlatform.Common.Filtering;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents the ability to track query filters or simple filters. Any attempt
/// to add a mix of the two will result in an exception.
/// </summary>
public class XunitFilters : ITestCaseFilter
{
	static readonly Version Version_1_0_0 = new(1, 0, 0);

	readonly XunitQueryFilters queryFilters = new();
	readonly XunitSimpleFilters simpleFilters = new();
	string? vstestFilter;

	/// <summary>
	/// Gets a flag indicating whether there are any active filters.
	/// </summary>
	public bool Empty =>
		queryFilters.Empty && simpleFilters.Empty;

	/// <summary>
	/// Adds a simple filter which excludes a fully qualified class name.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddExcludedClassFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddExcludedClassFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which excludes a test case display name.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.<br />
	/// <br />
	/// Note: Display name filters are supported by all v1 and v2 projects. Support for v3 projects
	/// requires targeting <c>xunit.v3.core</c> version <c>4.0.0</c> or later (these filters will
	/// be ignored for older versions of <c>xunit.v3.core</c>).
	/// </remarks>
	public void AddExcludedDisplayNameFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddExcludedDisplayNameFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which excludes a fully qualified method name. A fully qualified
	/// method name is in the form of <c>"FullyQualifiedTypeName.MethodName"</c>.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddExcludedMethodFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddExcludedMethodFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which excludes a namespace.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddExcludedNamespaceFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddExcludedNamespaceFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which excludes tests with the given name/value pair.
	/// </summary>
	/// <param name="name">The name of the trait</param>
	/// <param name="value">The value of the trait</param>
	/// <remarks>
	/// The name and/or value may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddExcludedTraitFilter(
		string name,
		string value)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddExcludedTraitFilter(name, value);
	}

	/// <summary>
	/// Adds a simple filter matching a fully qualified class name.
	/// </summary>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddIncludedClassFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddIncludedClassFilter(query);
	}

	/// <summary>
	/// Adds a simple filter matching a test case display name.
	/// </summary>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.<br />
	/// <br />
	/// Note: Display name filters are supported by all v1 and v2 projects. Support for v3 projects
	/// requires targeting <c>xunit.v3.core</c> version <c>4.0.0</c> or later (these filters will
	/// be ignored for older versions of <c>xunit.v3.core</c>).
	/// </remarks>
	public void AddIncludedDisplayNameFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddIncludedDisplayNameFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which matches a fully qualified method name. A fully qualified
	/// method name is in the form of <c>"FullyQualifiedTypeName.MethodName"</c>.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddIncludedMethodFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddIncludedMethodFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which matches a namespace.
	/// </summary>
	/// <param name="query">The filter query</param>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddIncludedNamespaceFilter(string query)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddIncludedNamespaceFilter(query);
	}

	/// <summary>
	/// Adds a simple filter which matches tests with the given name/value pair.
	/// </summary>
	/// <param name="name">The name of the trait</param>
	/// <param name="value">The value of the trait</param>
	/// <remarks>
	/// The name and/or value may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddIncludedTraitFilter(
		string name,
		string value)
	{
		GuardEmptyQueryFilters();
		GuardEmptyVSTestFilter();
		simpleFilters.AddIncludedTraitFilter(name, value);
	}

	/// <summary>
	/// Adds a query filter.
	/// </summary>
	/// <remarks>
	/// For more information on the query syntax, see <see href="https://xunit.net/docs/query-filter-language"/>
	/// </remarks>
	public void AddQueryFilter(string query)
	{
		GuardEmptySimpleFilters();
		GuardEmptyVSTestFilter();
		queryFilters.AddQueryFilter(query);
	}

	/// <summary>
	/// Adds as VSTest filter
	/// </summary>
	public void SetVSTestFilter(string vstestFilter)
	{
		GuardEmptyQueryFilters();
		GuardEmptySimpleFilters();
		GuardEmptyVSTestFilter("VSTest filter can only be set a single time");
		this.vstestFilter = vstestFilter;
	}

	/// <inheritdoc/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
	{
		if (!queryFilters.Empty)
			return queryFilters.Filter(assemblyName, testCase);

		if (!simpleFilters.Empty)
			return simpleFilters.Filter(assemblyName, testCase);

		if (vstestFilter is null)
			return true;

		var filterExpression = new TestCaseFilterExpression(new FilterExpressionWrapper(vstestFilter));
		return filterExpression.MatchTestCase(propertyName =>
		{
			if (string.Equals(propertyName, "FullyQualifiedName", StringComparison.OrdinalIgnoreCase))
			{
				if (testCase.TestClassName is null || testCase.TestMethodName is null)
					return null;

				return $"{testCase.TestClassName}.{testCase.TestMethodName}";
			}
			else if (string.Equals(propertyName, "DisplayName", StringComparison.OrdinalIgnoreCase))
				return testCase.TestCaseDisplayName;

			_ = testCase.Traits.TryGetValue(propertyName, out var values);
			return values?.ToArray();
		});
	}

	void GuardEmptyQueryFilters()
	{
		if (!queryFilters.Empty)
			throw new ArgumentException("Cannot add simple filter or VSTest filter; query filters already exist", "query");
	}

	void GuardEmptySimpleFilters()
	{
		if (!simpleFilters.Empty)
			throw new ArgumentException("Cannot add query filter or VSTest filter; simple filters already exist", "query");
	}

	void GuardEmptyVSTestFilter(string? message = null)
	{
		if (vstestFilter is not null)
#pragma warning disable CA2208  // We're guarding a parameter up the call stack, so we can't use nameof
			throw new ArgumentException(message ?? "Cannot add simple filter or query filter; VSTest filter already exists", "query");
#pragma warning restore CA2208
	}

	/// <summary>
	/// Please call <see cref="ToXunit3Arguments(Version)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	/// <remarks>
	/// This calls <see cref="ToXunit3Arguments(Version)"/> with a version of <c>1.0.0</c>.
	/// </remarks>
	[Obsolete("Please call the overload which accepts a Version. This overload will be removed in the next major version.")]
	public IReadOnlyCollection<string> ToXunit3Arguments() =>
		ToXunit3Arguments(Version_1_0_0);

	/// <summary>
	/// Gets the command-line arguments to pass to an xUnit.net v3 test assembly to perform
	/// the filtering contained within this filter.
	/// </summary>
	/// <param name="coreFrameworkVersion">The version of <c>xunit.v3.core</c> that is used. Will filter out
	/// command line options that aren't available for the given version.</param>
	public IReadOnlyCollection<string> ToXunit3Arguments(Version coreFrameworkVersion) =>
		!simpleFilters.Empty
			? simpleFilters.ToXunit3Arguments(coreFrameworkVersion)
			: !queryFilters.Empty
				? queryFilters.ToXunit3Arguments()
				: vstestFilter is not null
					? ["-filterVSTest", vstestFilter]
					: [];
}
