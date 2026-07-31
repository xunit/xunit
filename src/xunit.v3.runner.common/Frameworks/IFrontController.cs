using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Represents a class which acts as a front controller for unit testing frameworks.
/// This allows runners to run tests from multiple unit testing frameworks (in particular,
/// hiding the differences between xUnit.net v1, v2, and v3 tests).
/// </summary>
public interface IFrontController : IFrontControllerDiscoverer
{
	/// <summary>
	/// Gets the maximum threads to use for running tests in parallel, if the user has overridden
	/// the default.
	/// </summary>
	int? MaxParallelThreads { get; }

	/// <summary>
	/// Gets the parallel algorithm, if the user has overridden the default.
	/// </summary>
	ParallelAlgorithm? ParallelAlgorithm { get; }

	/// <summary>
	/// Gets the parallelization mode, if the user has overridden the default.
	/// </summary>
	ParallelMode? ParallelMode { get; }

	/// <summary>
	/// Starts the process of finding and running tests in an assembly. Typically only used
	/// by runner which do not present test discovery UIs to users that allow them to run
	/// selected tests (those should instead use <see cref="IFrontControllerDiscoverer.Find"/>
	/// and <see cref="Run"/> as separate operations).
	/// </summary>
	/// <param name="messageSink">The message sink to report results back to.</param>
	/// <param name="settings">The settings used during discovery and execution.</param>
	void FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings
	);

	/// <summary>
	/// Starts the process of running selected tests in the assembly. The serialized test
	/// cases to run come from calling <see cref="IFrontControllerDiscoverer.Find"/>.
	/// </summary>
	/// <param name="messageSink">The message sink to report results back to.</param>
	/// <param name="settings">The settings used during execution.</param>
	void Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings
	);
}
