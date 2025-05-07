using System;
using System.Linq;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class FilterTrait(
	string name,
	string value) :
		ITestCaseFilter
{
	readonly Func<string?, bool> nameEvaluator = QueryFilterParser.ToEvaluator(name);
	readonly Func<string?, bool> valueEvaluator = QueryFilterParser.ToEvaluator(value);

	/// <summary/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
	{
		Guard.ArgumentNotNull(testCase);

		foreach (var matchingNameKvp in testCase.Traits.Where(kvp => nameEvaluator(kvp.Key)))
			if (matchingNameKvp.Value.Any(valueEvaluator))
				return true;

		return false;
	}
}
