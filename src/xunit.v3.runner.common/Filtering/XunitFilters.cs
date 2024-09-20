using System;
using System.Collections.Generic;
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

	/// <summary>
	/// Gets a flag indicating whether there are any active filters.
	/// </summary>
	public bool Empty =>
		queryFilters.Empty && simpleFilters.Empty;

	/// <summary>
	/// Adds a simple filter which excludes a fully qualified class name.
	/// </summary>
	/// <remarks>
	/// The query may begin and/or end with <c>*</c> to add as a wildcard. No other wildcards
	/// are permitted in any other locations.
	/// </remarks>
	public void AddExcludedClassFilter(string query)
	{
		GuardEmptyQueryFilters();
		simpleFilters.AddExcludedClassFilter(query);
	}

	/// <summary/>
	public void AddExcludedMethodFilter(string query)
	{
		GuardEmptyQueryFilters();
		simpleFilters.AddExcludedMethodFilter(query);
	}

	/// <summary/>
	public void AddExcludedNamespaceFilter(string query)
	{
		GuardEmptyQueryFilters();
		simpleFilters.AddExcludedNamespaceFilter(query);
	}

	/// <summary/>
	public void AddExcludedTraitFilter(
		string name,
		string value)
	{
		GuardEmptyQueryFilters();
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

	/// <summary/>
	public void AddIncludedMethodFilter(string query)
	{
		GuardEmptyQueryFilters();
		simpleFilters.AddIncludedMethodFilter(query);
	}

	/// <summary/>
	public void AddIncludedNamespaceFilter(string query)
	{
		GuardEmptyQueryFilters();
		simpleFilters.AddIncludedNamespaceFilter(query);
	}

	/// <summary/>
	public void AddIncludedTraitFilter(
		string name,
		string value)
	{
		GuardEmptyQueryFilters();
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
		queryFilters.AddQueryFilter(query);
	}

	/// <inheritdoc/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			!queryFilters.Empty
				? queryFilters.Filter(assemblyName, testCase)
				: simpleFilters.Empty || simpleFilters.Filter(assemblyName, testCase);

	void GuardEmptyQueryFilters()
	{
		if (!queryFilters.Empty)
			throw new ArgumentException("Cannot add simple filter; query filters already exist", "query");
	}

	void GuardEmptySimpleFilters()
	{
		if (!simpleFilters.Empty)
			throw new ArgumentException("Cannot add query filter; simple filters already exist", "query");
	}

	/// <summary>
	/// Gets the command-line arguments to pass to an xUnit.net v3 test assembly to perform
	/// the filtering contained within this filter.
	/// </summary>
	public IReadOnlyCollection<string> ToXunit3Arguments() =>
		!simpleFilters.Empty
			? simpleFilters.ToXunit3Arguments()
			: !queryFilters.Empty
				? queryFilters.ToXunit3Arguments()
				: [];
}
