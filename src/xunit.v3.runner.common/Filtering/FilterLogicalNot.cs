using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterLogicalNot(ITestCaseFilter innerFilter) :
	ITestCaseFilter
{
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase) =>
			!innerFilter.Filter(assemblyName, testCase);
}
