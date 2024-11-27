using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCaseRunner"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
public class XunitTestCaseRunnerContext(
	IXunitTestCase testCase,
	IReadOnlyCollection<IXunitTest> tests,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	string displayName,
	string? skipReason,
	ExplicitOption explicitOption,
	object?[] constructorArguments) :
		XunitTestCaseRunnerBaseContext<IXunitTestCase, IXunitTest>(testCase, tests, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, explicitOption, constructorArguments)
{ }
