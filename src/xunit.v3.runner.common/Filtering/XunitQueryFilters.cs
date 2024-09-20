using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal class XunitQueryFilters : ITestCaseFilter
{
	readonly FilterLogicalOr baseFilter = new();
	readonly List<string> xunit3Arguments = [];

	public bool Empty =>
		baseFilter.Filters.Count == 0;

	public void AddQueryFilter(string query)
	{
		baseFilter.Filters.Add(QueryFilterParser.Parse(query));
		xunit3Arguments.AddRange(["-filter", query]);
	}

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			baseFilter.Filter(assemblyName, testCase);

	public IReadOnlyCollection<string> ToXunit3Arguments() =>
		xunit3Arguments;
}
