using Xunit.Internal;
using Xunit.Sdk;

// TODO: Should this have an interface, to match up with the core messages usage?

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner has just finished discovery for a test assembly. This message will
/// arrive after the test framework's <see cref="IDiscoveryComplete"/> message, and contains
/// the project metadata associated with the discovery.
/// </summary>
/// <remarks>
/// This message does not support serialization or deserialization.
/// </remarks>
public class TestAssemblyDiscoveryFinished : IMessageSinkMessage
{
	XunitProjectAssembly? assembly;
	ITestFrameworkDiscoveryOptions? discoveryOptions;

	/// <summary>
	/// Gets information about the assembly being discovered.
	/// </summary>
	public required XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that were used during discovery.
	/// </summary>
	public required ITestFrameworkDiscoveryOptions DiscoveryOptions
	{
		get => this.ValidateNullablePropertyValue(discoveryOptions, nameof(DiscoveryOptions));
		set => discoveryOptions = Guard.ArgumentNotNull(value, nameof(DiscoveryOptions));
	}

	/// <summary>
	/// Gets the count of the number of test cases that will be run (post-filtering).
	/// </summary>
	public int TestCasesToRun { get; set; }

	/// <inheritdoc/>
	public string? ToJson() =>
		null;
}
