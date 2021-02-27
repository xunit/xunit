using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Represents a class which acts as a front controller for unit testing frameworks.
	/// This allows runners to run tests from multiple unit testing frameworks (in particular,
	/// hiding the differences between xUnit.net v1, v2, and v3 tests).
	/// </summary>
	public interface IFrontController : IFrontControllerDiscoverer
	{
		/// <summary>
		/// Starts the process of finding and running tests in an assembly. Typically only used
		/// by runner which do not present test discovery UIs to users that allow them to run
		/// selected tests (those should instead use <see cref="IFrontControllerDiscoverer.Find"/>
		/// and <see cref="Run"/> as separate operations).
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="settings">The settings used during discovery and execution.</param>
		void FindAndRun(
			_IMessageSink messageSink,
			FrontControllerFindAndRunSettings settings
		);

		/// <summary>
		/// Starts the process of running selected tests in the assembly. The serialized test
		/// cases to run come from calling <see cref="IFrontControllerDiscoverer.Find"/>.
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="settings">The settings used during execution.</param>
		void Run(
			_IMessageSink messageSink,
			FrontControllerRunSettings settings
		);
	}
}
