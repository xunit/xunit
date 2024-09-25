using Xunit.Internal;
using Xunit.Sdk;

// TODO: Should this have an interface, to match up with the core messages usage?

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start discovery for a test assembly. This message will
/// arrive before the test framework's <see cref="T:Xunit.DiscoveryStarting"/> message, and
/// contains the project metadata associated with the discovery.
/// </summary>
/// <remarks>
/// This message does not support serialization or deserialization.
/// </remarks>
public class TestAssemblyDiscoveryStarting : IMessageSinkMessage
{
	AppDomainOption? appDomain;
	XunitProjectAssembly? assembly;
	ITestFrameworkDiscoveryOptions? discoveryOptions;

	/// <summary>
	/// Gets a flag which indicates whether the tests will be discovered and run in a
	/// separate app domain.
	/// </summary>
	public required AppDomainOption AppDomain
	{
		get => this.ValidateNullablePropertyValue(appDomain, nameof(AppDomain));
		set => appDomain = value;
	}

	/// <summary>
	/// Gets information about the assembly being discovered.
	/// </summary>
	public required XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that will be used during discovery.
	/// </summary>
	public required ITestFrameworkDiscoveryOptions DiscoveryOptions
	{
		get => this.ValidateNullablePropertyValue(discoveryOptions, nameof(DiscoveryOptions));
		set => discoveryOptions = Guard.ArgumentNotNull(value, nameof(DiscoveryOptions));
	}

	/// <summary>
	/// Gets a flag which indicates whether shadow copies are being used. If app domains are
	/// not enabled, then this value is ignored.
	/// </summary>
	public required bool ShadowCopy { get; set; }

	/// <inheritdoc/>
	public string? ToJson() =>
		null;
}
