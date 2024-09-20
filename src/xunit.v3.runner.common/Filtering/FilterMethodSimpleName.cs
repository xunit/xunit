using System;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterMethodSimpleName(string filter) :
	ITestCaseFilter
{
	readonly Func<string?, bool> evaluator = QueryFilterParser.ToEvaluator(filter);

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			evaluator(testCase.TestMethodName);
}
