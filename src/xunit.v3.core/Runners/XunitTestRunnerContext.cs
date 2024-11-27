using System.Collections.Generic;
using System.Threading;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
/// <param name="test">The test</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="beforeAfterTestAttributes">The <see cref="IBeforeAfterTestAttribute"/>s that are applied to the test</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
public class XunitTestRunnerContext(
	IXunitTest test,
	IMessageBus messageBus,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
	object?[] constructorArguments) :
		XunitTestRunnerBaseContext<IXunitTest>(test, messageBus, explicitOption, aggregator, cancellationTokenSource, beforeAfterTestAttributes, constructorArguments)
{ }
