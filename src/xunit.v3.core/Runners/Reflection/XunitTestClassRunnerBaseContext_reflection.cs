using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestClassRunnerBase{TContext, TTestClass, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="collectionFixtureMappings">The fixtures attached to the test collection</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestClassRunnerBaseContext<TTestClass, TTestMethod, TTestCase>(
	TTestClass testClass,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager collectionFixtureMappings) :
		CoreTestClassRunnerContext<TTestClass, TTestMethod, TTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestClass : class, IXunitTestClass
			where TTestMethod : class, IXunitTestMethod
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Gets the mapping manager for class-level fixtures.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; } = new("Class", Guard.ArgumentNotNull(collectionFixtureMappings));

	/// <summary>
	/// Gets or sets the constructor arguments used during test class creation.
	/// </summary>
	public object?[]? ConstructorArguments { get; set; }

	/// <remarks>
	/// If <see cref="ConstructorArguments"/> has not been set, this will throw <see cref="InvalidOperationException"/>.
	/// It is the runner's responsibility to create the constructor arguments and assign them into the context before
	/// attempting to run any test methods.
	/// </remarks>
	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestMethod(
		TTestMethod testMethod,
		IReadOnlyCollection<TTestCase> testCases) =>
			XunitTestMethodRunner.Instance.Run(
				testMethod,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				ConstructorArguments ?? throw new InvalidOperationException("Constructor arguments were not set")
			);
}
