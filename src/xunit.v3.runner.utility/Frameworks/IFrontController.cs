using System.Collections.Generic;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Represents a class which acts as a front controller for unit testing frameworks.
	/// This allows runners to run tests from multiple unit testing frameworks (in particular,
	/// hiding the differences between xUnit.net v1, v2, and v3 tests).
	/// </summary>
	public interface IFrontController
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
		/// Starts the process of finding all tests in an assembly.
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="settings">The settings used during discovery.</param>
		void Find(
			_IMessageSink messageSink,
			FrontControllerDiscoverySettings settings
		);

		/// <summary>
		/// Starts the process of running all the tests in the assembly.
		/// </summary>
		/// <param name="executionMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options to be used during test discovery.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		void RunAll(
			_IMessageSink executionMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions
		);

		/// <summary>
		/// Starts the process of running selected tests in the assembly.
		/// </summary>
		/// <param name="serializedTestCases">The test cases to run.</param>
		/// <param name="executionMessageSink">The message sink to report results back to.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		void RunTests(
			IEnumerable<string> serializedTestCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions
		);
	}
}
