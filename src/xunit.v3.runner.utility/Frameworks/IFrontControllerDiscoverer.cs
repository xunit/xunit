using System;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Represents a class which acts as a front controller for unit testing frameworks for
	/// the purposes of discovery (which can include source-based discovery). Tests found
	/// with these classes can later be run by an instance of <see cref="IFrontController"/>.
	/// This allows runners to run tests from multiple unit testing frameworks (in particular,
	/// hiding the differences between xUnit.net v1, v2, and v3 tests).
	/// </summary>
	public interface IFrontControllerDiscoverer : IAsyncDisposable
	{
		/// <summary>
		/// Gets a flag indicating whether this discovery/execution can use app domains.
		/// </summary>
		bool CanUseAppDomains { get; }

		/// <summary>
		/// Gets the target framework that the test assembly is linked against.
		/// </summary>
		string TargetFramework { get; }

		/// <summary>
		/// Gets the unique ID for the test assembly provided to the discoverer.
		/// </summary>
		string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Returns the display name of the test framework that this discoverer is running tests for.
		/// </summary>
		string TestFrameworkDisplayName { get; }

		/// <summary>
		/// Starts the process of finding tests in an assembly. Typically only used by
		/// runners which discover tests and present them into a UI for the user to interactively
		/// choose for selective run (via <see cref="IFrontController.Run"/>). For runners which
		/// simply wish to discover and immediately run tests, they should instead
		/// use <see cref="IFrontController.FindAndRun"/>, which permits the same filtering logic
		/// as this method.
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="settings">The settings used during discovery.</param>
		void Find(
			_IMessageSink messageSink,
			FrontControllerFindSettings settings
		);
	}
}
