using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Reports that runner is about to start discovery for a test assembly.
/// </summary>
public class TestAssemblyDiscoveryStarting : _MessageSinkMessage
{
	AppDomainOption? appDomain;
	XunitProjectAssembly? assembly;
	_ITestFrameworkDiscoveryOptions? discoveryOptions;

	/// <summary>
	/// Gets a flag which indicates whether the tests will be discovered and run in a
	/// separate app domain.
	/// </summary>
	public AppDomainOption AppDomain
	{
		get => this.ValidateNullablePropertyValue(appDomain, nameof(AppDomain));
		set => appDomain = value;
	}

	/// <summary>
	/// Gets information about the assembly being discovered.
	/// </summary>
	public XunitProjectAssembly Assembly
	{
		get => this.ValidateNullablePropertyValue(assembly, nameof(Assembly));
		set => assembly = Guard.ArgumentNotNull(value, nameof(Assembly));
	}

	/// <summary>
	/// Gets the options that will be used during discovery.
	/// </summary>
	public _ITestFrameworkDiscoveryOptions DiscoveryOptions
	{
		get => this.ValidateNullablePropertyValue(discoveryOptions, nameof(DiscoveryOptions));
		set => discoveryOptions = Guard.ArgumentNotNull(value, nameof(DiscoveryOptions));
	}

	/// <summary>
	/// Gets a flag which indicates whether shadow copies are being used. If app domains are
	/// not enabled, then this value is ignored.
	/// </summary>
	public bool ShadowCopy { get; set; }
}
