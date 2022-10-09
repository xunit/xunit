using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Contains the information by <see cref="IFrontController.FindAndRun"/>.
/// </summary>
public class FrontControllerFindAndRunSettings
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FrontControllerFindSettings"/> class.
	/// </summary>
	/// <param name="discoveryOptions">The options used during discovery</param>
	/// <param name="executionOptions">The options used during execution</param>
	/// <param name="filters">The optional filters (when not provided, finds all tests)</param>
	public FrontControllerFindAndRunSettings(
		_ITestFrameworkDiscoveryOptions discoveryOptions,
		_ITestFrameworkExecutionOptions executionOptions,
		XunitFilters? filters = null)
	{
		DiscoveryOptions = Guard.ArgumentNotNull(discoveryOptions);
		ExecutionOptions = Guard.ArgumentNotNull(executionOptions);
		Filters = filters ?? new XunitFilters();
	}

	/// <summary>
	/// The options used during discovery.
	/// </summary>
	public _ITestFrameworkDiscoveryOptions DiscoveryOptions { get; }

	/// <summary>
	/// The options used during execution.
	/// </summary>
	public _ITestFrameworkExecutionOptions ExecutionOptions { get; }

	/// <summary>
	/// Get the test case filters used during discovery.
	/// </summary>
	public XunitFilters Filters { get; }
}
