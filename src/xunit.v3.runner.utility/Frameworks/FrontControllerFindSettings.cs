using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontControllerDiscoverer.Find"/>.
/// </summary>
/// <param name="options">The discovery options</param>
/// <param name="filters">The optional filters (when not provided, finds all tests)</param>
public class FrontControllerFindSettings(
	ITestFrameworkDiscoveryOptions options,
	XunitFilters? filters = null) :
		FrontControllerSettingsBase
{
	/// <summary>
	/// Get the test case filters used during discovery.
	/// </summary>
	public XunitFilters Filters { get; } = filters ?? new XunitFilters();

	/// <summary>
	/// The options used during discovery.
	/// </summary>
	public ITestFrameworkDiscoveryOptions Options { get; } = Guard.ArgumentNotNull(options);
}
