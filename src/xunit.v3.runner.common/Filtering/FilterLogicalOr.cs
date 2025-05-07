using System.Collections.Generic;
using System.Linq;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class FilterLogicalOr(params ITestCaseFilter[] filters) :
	ITestCaseFilter, ITestCaseFilterComposite
{
	/// <summary/>
	public List<ITestCaseFilter> Filters { get; } = filters.ToList();

	/// <summary/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			Filters.Count == 0 || Filters.Any(filter => filter.Filter(assemblyName, testCase));
}
