using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontControllerDiscoverer.Find"/>.
/// </summary>
// TODO: This is currently ctor-based because everything else uses factories,
// are we comfortable with that design?
public class FrontControllerFindSettings
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FrontControllerFindSettings"/> class.
	/// </summary>
	/// <param name="options">The discovery options</param>
	/// <param name="filters">The optional filters (when not provided, finds all tests)</param>
	public FrontControllerFindSettings(
		_ITestFrameworkDiscoveryOptions options,
		XunitFilters? filters = null)
	{
		Options = Guard.ArgumentNotNull(options);
		Filters = filters ?? new XunitFilters();
	}

	/// <summary>
	/// Get the test case filters used during discovery.
	/// </summary>
	public XunitFilters Filters { get; }

	/// <summary>
	/// The options used during discovery.
	/// </summary>
	public _ITestFrameworkDiscoveryOptions Options { get; }
}
