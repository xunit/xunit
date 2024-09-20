using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a filter run against a test case (and the associated assembly it resides in).
/// </summary>
public interface ITestCaseFilter
{
	/// <summary>
	/// Determines whether the given <paramref name="testCase"/> passes the filter.
	/// </summary>
	/// <param name="assemblyName">The simple assembly name without file extension</param>
	/// <param name="testCase">The test case to be checked against the filter</param>
	bool Filter(
		string assemblyName,
		ITestCaseMetadata testCase);
}
