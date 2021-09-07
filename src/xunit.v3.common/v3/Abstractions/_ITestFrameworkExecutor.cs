using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xunit.v3
{
	/// <summary>
	/// Represents an implementation of the execution part of a test framework. Implementations may
	/// optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
	/// for cleanup operations.
	/// </summary>
	public interface _ITestFrameworkExecutor
	{
		/// <summary>
		/// Runs all tests in the assembly.
		/// </summary>
		/// <param name="executionMessageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options to be used during test discovery.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		ValueTask RunAll(
			_IMessageSink executionMessageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions
		);

		/// <summary>
		/// Runs selected test cases in the assembly.
		/// </summary>
		/// <param name="testCases">The test cases to run.</param>
		/// <param name="executionMessageSink">The message sink to report results back to.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		ValueTask RunTestCases(
			IReadOnlyCollection<_ITestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions
		);
	}
}
