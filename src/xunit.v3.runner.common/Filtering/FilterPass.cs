using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public sealed class FilterPass : ITestCaseFilter
{
	FilterPass()
	{ }

	/// <summary/>
	public static FilterPass Instance { get; } = new();

	/// <summary/>
	public bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase)
			=> true;
}
