using System;
using System.Linq;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterTrait(
	string name,
	string value) :
		ITestCaseFilter
{
	readonly Func<string?, bool> nameEvaluator = QueryFilterParser.ToEvaluator(name);
	readonly Func<string?, bool> valueEvaluator = QueryFilterParser.ToEvaluator(value);

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
	{
		foreach (var matchingNameKvp in testCase.Traits.Where(kvp => nameEvaluator(kvp.Key)))
			if (matchingNameKvp.Value.Any(valueEvaluator))
				return true;

		return false;
	}
}
