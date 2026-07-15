using System.ComponentModel;
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
/// <param name="parallelMode">The parallel mode for the test class</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
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
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	FixtureMappingManager collectionFixtureMappings) :
		CoreTestClassRunnerContext<TTestClass, TTestMethod, TTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
			where TTestClass : class, IXunitTestClass
			where TTestMethod : class, IXunitTestMethod
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Please call <see cref="XunitTestClassRunnerBaseContext{TTestClass, TTestMethod, TTestCase}.XunitTestClassRunnerBaseContext(TTestClass, IReadOnlyCollection{TTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This constructor is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This constructor is no longer valid and will be removed in the next major version.", error: true)]
	protected XunitTestClassRunnerBaseContext(
		TTestClass testClass,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) :
			this(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, collectionFixtureMappings) =>
				throw new NotSupportedException("Please call the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This constructor is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Gets the mapping manager for class-level fixtures.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; } = new("Class", Guard.ArgumentNotNull(collectionFixtureMappings));

	/// <summary>
	/// Gets or sets the constructor arguments used during test class creation.
	/// </summary>
	public object?[]? ConstructorArguments { get; set; }

	/// <summary>
	/// The optional test case orderer for the test class can be retrieved from <c>TestClass.TestCaseOrderer</c>.
	/// This property is no longer valid, and will be removed in the next major version.
	/// </summary>
	[Obsolete("The optional test case orderer for the test class can be retrieved from from TestClass.TestCaseOrderer. This property is no longer valid, and will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ITestCaseOrderer TestCaseOrderer
	{
		get => TestClass.TestCaseOrderer ?? TestClass.TestCollection.TestCaseOrderer ?? TestClass.TestCollection.TestAssembly.TestCaseOrderer ?? DefaultTestCaseOrderer.Instance;
		set => throw new NotSupportedException("This property is no longer valid, and will be removed in the next major version.");
	}

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
				ParallelMode,
				Scheduler,
				ConstructorArguments ?? throw new InvalidOperationException("Constructor arguments were not set"),
				ClassFixtureMappings
			);
}
