using System;
using System.Collections.Generic;

namespace Xunit.v3
{
	/// <summary>
	/// Represents an implementation of the execution part of a test framework.
	/// Implementations may optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
	/// for cleanup operations.
	/// </summary>
	public interface _ITestFrameworkExecutor
	{
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
