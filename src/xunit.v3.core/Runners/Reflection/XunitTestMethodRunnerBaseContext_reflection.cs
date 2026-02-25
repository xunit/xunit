using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunnerBase{TContext, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>(
	TTestMethod testMethod,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	object?[] constructorArguments) :
		CoreTestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestMethod : class, IXunitTestMethod
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Gets the arguments to send to the test class constructor.
	/// </summary>
	public object?[] ConstructorArguments { get; } = Guard.ArgumentNotNull(constructorArguments);

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestCase(TTestCase testCase)
	{
		if (testCase is ISelfExecutingXunitTestCase selfExecutingTestCase)
			return selfExecutingTestCase.Run(ExplicitOption, MessageBus, ConstructorArguments, Aggregator.Clone(), CancellationTokenSource);

		return XunitRunnerHelper.RunXunitTestCase(
			testCase,
			MessageBus,
			CancellationTokenSource,
			Aggregator.Clone(),
			ExplicitOption,
			ConstructorArguments
		);
	}
}
