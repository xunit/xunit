using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents the ability to track query filters or simple filters. Any attempt
/// to add a mix of the two will result in an exception.
/// </summary>
public class XunitFilters : ITestCaseFilter
{
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
		simpleFilters.AddIncludedClassFilter(query);
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
		this.vstestFilter = vstestFilter;
	}

	/// <inheritdoc/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
	{
		if (!queryFilters.Empty)
		{
			return queryFilters.Filter(assemblyName, testCase);
		}
		else if (!simpleFilters.Empty)
		{
			return simpleFilters.Filter(assemblyName, testCase);
		}
		else if (vstestFilter is not null)
		{
			var filterExpression = new TestCaseFilterExpression(new FilterExpressionWrapper(vstestFilter));

			return filterExpression.MatchTestCase(propertyName =>
			{
				if (string.Equals(propertyName, "FullyQualifiedName", StringComparison.OrdinalIgnoreCase))
				{
					if (testCase.TestClassName is null || testCase.TestMethodName is null)
					{
						return null;
					}

					return $"{testCase.TestClassName}.{testCase.TestMethodName}"; // TODO: Should we add backtick and arity here?
				}
				else if (string.Equals(propertyName, "DisplayName", StringComparison.OrdinalIgnoreCase))
				{
					return testCase.TestCaseDisplayName;
				}

				_ = testCase.Traits.TryGetValue(propertyName, out var values);
				return values?.ToArray();
			});
		}

		return true;
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

	void GuardEmptyVSTestFilter()
	{
		if (vstestFilter is not null)
			throw new ArgumentException("Cannot add simple filter or query filter; VSTest filter already exist", "query");
	}

	/// <summary>
	/// Gets the command-line arguments to pass to an xUnit.net v3 test assembly to perform
	/// the filtering contained within this filter.
	/// </summary>
	// TODO: This is for xunit runner and not for MTP? Consider either supporting VSTest filter there or alternatively just throw when it's set as not supported?
	public IReadOnlyCollection<string> ToXunit3Arguments() =>
		!simpleFilters.Empty
			? simpleFilters.ToXunit3Arguments()
			: !queryFilters.Empty
				? queryFilters.ToXunit3Arguments()
				: [];
}
