using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterFail : ITestCaseFilter
{
	FilterFail()
	{ }

	public static FilterFail Instance { get; } = new();

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
			=> false;
}
