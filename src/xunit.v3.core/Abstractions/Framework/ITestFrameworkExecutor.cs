using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Represents an implementation of the execution part of a test framework. Implementations may
/// optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
/// for cleanup operations.
/// </summary>
public interface ITestFrameworkExecutor
{
	/// <summary>
	/// Runs selected test cases in the assembly.
	/// </summary>
	/// <param name="testCases">The test cases to run.</param>
	/// <param name="executionMessageSink">The message sink to report results back to.</param>
	/// <param name="executionOptions">The options to be used during test execution.</param>
	/// <param name="cancellationToken">The optional cancellation token which can be used to cancel the test
	/// execution process.</param>
	ValueTask RunTestCases(
		IReadOnlyCollection<ITestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken? cancellationToken = null
	);
}
