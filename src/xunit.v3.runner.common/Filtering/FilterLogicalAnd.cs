using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterLogicalAnd(params ITestCaseFilter[] filters) :
	ITestCaseFilter, ITestCaseFilterComposite
{
	public List<ITestCaseFilter> Filters { get; } = filters.ToList();

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			Filters.Count == 0 || Filters.All(filter => filter.Filter(assemblyName, testCase));
}
