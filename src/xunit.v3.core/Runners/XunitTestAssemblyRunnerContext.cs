using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestAssemblyRunner"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
public class XunitTestAssemblyRunnerContext(
	IXunitTestAssembly testAssembly,
	IReadOnlyCollection<IXunitTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		XunitTestAssemblyRunnerBaseContext<IXunitTestAssembly, IXunitTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{ }
