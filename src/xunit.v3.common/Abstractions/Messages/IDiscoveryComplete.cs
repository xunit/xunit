namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the discovery process has been completed for
/// the requested assembly.
/// </summary>
public interface IDiscoveryComplete : ITestAssemblyMessage
{
	/// <summary>
	/// Gets a count of the number of test cases that passed the filter and will be run.
	/// </summary>
	int TestCasesToRun { get; }
}
