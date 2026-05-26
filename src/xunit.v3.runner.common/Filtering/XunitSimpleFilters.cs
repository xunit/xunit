using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class XunitSimpleFilters : ITestCaseFilter
{
	static readonly Version Version_0_0_0 = new(0, 0, 0);
	static readonly Version Version_4_0_0 = new(4, 0, 0);

	readonly FilterLogicalAnd baseFilter = new();
	readonly Lazy<FilterLogicalOr> excludedClassFilter;
	readonly Lazy<FilterLogicalOr> excludedDisplayNameFilter;
	readonly Lazy<FilterLogicalOr> excludedMethodFilter;
	readonly Lazy<FilterLogicalOr> excludedNamespaceFilter;
	readonly Lazy<FilterLogicalOr> excludedTraitFilter;
	readonly Lazy<FilterLogicalOr> includedClassFilter;
	readonly Lazy<FilterLogicalOr> includedDisplayNameFilter;
	readonly Lazy<FilterLogicalOr> includedMethodFilter;
	readonly Lazy<FilterLogicalOr> includedNamespaceFilter;
	readonly Lazy<FilterLogicalOr> includedTraitFilter;
	readonly List<(Version MinVersion, string[] Arguments)> xunit3Arguments = [];

	public XunitSimpleFilters()
	{
		excludedClassFilter = new(CreateNotOrFilter, isThreadSafe: false);
		excludedDisplayNameFilter = new(CreateNotOrFilter, isThreadSafe: false);
		excludedMethodFilter = new(CreateNotOrFilter, isThreadSafe: false);
		excludedNamespaceFilter = new(CreateNotOrFilter, isThreadSafe: false);
		excludedTraitFilter = new(CreateNotOrFilter, isThreadSafe: false);
		includedClassFilter = new(CreateOrFilter, isThreadSafe: false);
		includedDisplayNameFilter = new(CreateOrFilter, isThreadSafe: false);
		includedMethodFilter = new(CreateOrFilter, isThreadSafe: false);
		includedNamespaceFilter = new(CreateOrFilter, isThreadSafe: false);
		includedTraitFilter = new(CreateOrFilter, isThreadSafe: false);

		FilterLogicalOr CreateOrFilter()
		{
			var result = new FilterLogicalOr();
			baseFilter.Filters.Add(result);
			return result;
		}

		FilterLogicalOr CreateNotOrFilter()
		{
			var result = new FilterLogicalOr();
			baseFilter.Filters.Add(new FilterLogicalNot(result));
			return result;
		}
	}

	public void AddExcludedClassFilter(string query)
	{
		excludedClassFilter.Value.Filters.Add(new FilterClassFullName(query));
		xunit3Arguments.Add((Version_0_0_0, ["-class-", query]));
	}

	public void AddExcludedDisplayNameFilter(string query)
	{
		excludedDisplayNameFilter.Value.Filters.Add(new FilterDisplayName(query));
		xunit3Arguments.Add((Version_4_0_0, ["-displayname-", query]));
	}

	public void AddExcludedMethodFilter(string query)
	{
		excludedMethodFilter.Value.Filters.Add(new FilterMethodFullName(query));
		xunit3Arguments.Add((Version_0_0_0, ["-method-", query]));
	}

	public void AddExcludedNamespaceFilter(string query)
	{
		excludedNamespaceFilter.Value.Filters.Add(new FilterNamespace(query));
		xunit3Arguments.Add((Version_0_0_0, ["-namespace-", query]));
	}

	public void AddExcludedTraitFilter(
		string name,
		string value)
	{
		excludedTraitFilter.Value.Filters.Add(new FilterTrait(name, value));
		xunit3Arguments.Add((Version_0_0_0, ["-trait-", $"{name}={value}"]));
	}

	public void AddIncludedClassFilter(string query)
	{
		includedClassFilter.Value.Filters.Add(new FilterClassFullName(query));
		xunit3Arguments.Add((Version_0_0_0, ["-class", query]));
	}

	public void AddIncludedDisplayNameFilter(string query)
	{
		includedDisplayNameFilter.Value.Filters.Add(new FilterDisplayName(query));
		xunit3Arguments.Add((Version_4_0_0, ["-displayname", query]));
	}

	public void AddIncludedMethodFilter(string query)
	{
		includedMethodFilter.Value.Filters.Add(new FilterMethodFullName(query));
		xunit3Arguments.Add((Version_0_0_0, ["-method", query]));
	}

	public void AddIncludedNamespaceFilter(string query)
	{
		includedNamespaceFilter.Value.Filters.Add(new FilterNamespace(query));
		xunit3Arguments.Add((Version_0_0_0, ["-namespace", query]));
	}

	public void AddIncludedTraitFilter(
		string name,
		string value)
	{
		includedTraitFilter.Value.Filters.Add(new FilterTrait(name, value));
		xunit3Arguments.Add((Version_0_0_0, ["-trait", $"{name}={value}"]));
	}

	public bool Empty =>
		baseFilter.Filters.Count == 0;

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			baseFilter.Filter(assemblyName, testCase);

	public IReadOnlyCollection<string> ToXunit3Arguments(Version coreFrameworkVersion) =>
		xunit3Arguments
			.Where(x => coreFrameworkVersion >= x.MinVersion)
			.SelectMany(x => x.Arguments)
			.CastOrToReadOnlyCollection();
}
