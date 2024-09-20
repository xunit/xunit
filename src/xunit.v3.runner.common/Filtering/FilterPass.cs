using Xunit.Sdk;

namespace Xunit.Runner.Common;

internal sealed class FilterPass : ITestCaseFilter
{
	FilterPass()
	{ }

	public static FilterPass Instance { get; } = new();

	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
			=> true;
}
