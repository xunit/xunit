using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestInvoker"/>.
/// </summary>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="test">The test</param>
/// <param name="testClassInstance">The test class instance</param>
public class XunitTestInvokerContext(
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	IXunitTest test,
	object? testClassInstance) :
		TestInvokerContext<IXunitTest>(
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			test,
			Guard.ArgumentNotNull(test).TestMethod.TestClass.Class,
			testClassInstance,
			test.TestMethod.Method,
			test.TestMethodArguments
		)
{ }
