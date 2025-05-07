using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class FilterFail : ITestCaseFilter
{
	FilterFail()
	{ }

	/// <summary/>
	public static FilterFail Instance { get; } = new();

	/// <summary/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
			=> false;
}
