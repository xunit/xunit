using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestClassRunnerContext{TTestCase, TTestClass}"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTestClass">The type of the test class used by the test framework.
/// Must derive from <see cref="ITestClass"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public class TestClassRunnerContext<TTestClass, TTestCase>(
	TTestClass testClass,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		ContextBase(explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCase : class, ITestCase
			where TTestClass : class, ITestClass
{
	/// <summary>
	/// Gets the test cases associated with the test class.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; } = Guard.ArgumentNotNull(testCases);

	/// <summary>
	/// Gets the test class that is being executed.
	/// </summary>
	public TTestClass TestClass { get; } = Guard.ArgumentNotNull(testClass);
}
