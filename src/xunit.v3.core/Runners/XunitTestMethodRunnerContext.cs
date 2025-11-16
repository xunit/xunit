using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunner"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="testCaseOrderer">The orderer used to sort the test cases in the method</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
public class XunitTestMethodRunnerContext(
	IXunitTestMethod testMethod,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ITestCaseOrderer testCaseOrderer,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	object?[] constructorArguments) :
		XunitTestMethodRunnerBaseContext<IXunitTestMethod, IXunitTestCase>(
			testMethod,
			testCases,
			explicitOption,
			messageBus,
			testCaseOrderer,
			aggregator,
			cancellationTokenSource,
			constructorArguments
		)
{ }
