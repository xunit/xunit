using System;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class FilterAssembly(string filter) :
	ITestCaseFilter
{
	readonly Func<string?, bool> evaluator = QueryFilterParser.ToEvaluator(filter);

	/// <summary/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			evaluator(assemblyName);
}
