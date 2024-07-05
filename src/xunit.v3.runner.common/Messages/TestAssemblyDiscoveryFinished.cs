using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner has just finished discovery for a test assembly. This message will
/// arrive after the test framework's "discovery finished" message (i.e., for the default
/// test framework, <see cref="_DiscoveryComplete"/>), and contains the project metadata
/// associated with the discovery.
/// </summary>
public class TestAssemblyDiscoveryFinished : _MessageSinkMessage
{
	XunitProjectAssembly? assembly;
	_ITestFrameworkDiscoveryOptions? discoveryOptions;

	/// <summary>
	/// Gets information about the assembly being discovered.
	/// </summary>
	public XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that were used during discovery.
	/// </summary>
	public _ITestFrameworkDiscoveryOptions DiscoveryOptions
	{
		get => this.ValidateNullablePropertyValue(discoveryOptions, nameof(DiscoveryOptions));
		set => discoveryOptions = Guard.ArgumentNotNull(value, nameof(DiscoveryOptions));
	}

	/// <summary>
	/// Gets the count of the number of test cases that will be run (post-filtering).
	/// </summary>
	public int TestCasesToRun { get; set; }
}
