using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontController.FindAndRun"/>.
/// </summary>
/// <param name="discoveryOptions">The options used during discovery</param>
/// <param name="executionOptions">The options used during execution</param>
/// <param name="filters">The optional filters (when not provided, finds all tests)</param>
public class FrontControllerFindAndRunSettings(
	ITestFrameworkDiscoveryOptions discoveryOptions,
	ITestFrameworkExecutionOptions executionOptions,
	XunitFilters? filters = null) :
		FrontControllerSettingsBase
{
	/// <summary>
	/// The options used during discovery.
	/// </summary>
	public ITestFrameworkDiscoveryOptions DiscoveryOptions { get; } = Guard.ArgumentNotNull(discoveryOptions);

	/// <summary>
	/// The options used during execution.
	/// </summary>
	public ITestFrameworkExecutionOptions ExecutionOptions { get; } = Guard.ArgumentNotNull(executionOptions);

	/// <summary>
	/// Get the test case filters used during discovery.
	/// </summary>
	public XunitFilters Filters { get; } = filters ?? new XunitFilters();
}
