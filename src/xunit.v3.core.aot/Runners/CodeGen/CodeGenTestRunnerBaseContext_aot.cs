using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestRunnerBase{TContext, TTest}"/>.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestRunnerBaseContext<TTest> : CoreTestRunnerContext<TTest, BeforeAfterTestAttribute>
	where TTest : class, ICodeGenTest
{
	// We want to cache the results of this, since it will potentially be called more than once,
	// and it involves invoking lambdas.
	readonly Lazy<string?> getRuntimeSkipReason;

	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestRunnerBaseContext{TTest}"/> class.
	/// </summary>
	/// <param name="test">The test</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="parallelMode">The parallel mode for the test</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="caseFixtureMappings">The mapping of test case fixture types to fixtures.</param>
	protected CodeGenTestRunnerBaseContext(
		TTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		FixtureMappingManager caseFixtureMappings) :
			base(Guard.ArgumentNotNull(test), explicitOption, messageBus, aggregator, test.SkipReason, cancellationTokenSource, parallelMode, scheduler)
	{
		BeforeAfterTestAttributes = Test.TestCase.TestMethod.BeforeAfterTestAttributes;
		TestFixtureMappings = Guard.ArgumentNotNull(caseFixtureMappings);

		getRuntimeSkipReason = new(SafeGetRuntimeSkipReason);
	}

	/// <inheritdoc/>
	protected override IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }

	/// <summary>
	/// Gets the mapping manager for test-level fixtures.
	/// </summary>
	/// <remarks>
	/// There is no mechanism for describing or attaching test-level fixtures at this time, so this currently
	/// returns the mapping manager for the class-level fixtures. If test-level fixtures become a feature in the
	/// future, it is anticipated that this API will return the test-level fixture mapping manager.
	/// </remarks>
	public FixtureMappingManager TestFixtureMappings { get; }

	/// <summary>
	/// Creates the test class instance.
	/// </summary>
	public virtual ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance() =>
		Aggregator.RunAsync(
			async () =>
			{
				var result = await Test.TestCase.TestClass.TestClassFactory(TestFixtureMappings);

				if (result.Instance is IAsyncLifetime asyncLifetime)
					await asyncLifetime.InitializeAsync();

				return (result.Instance, result.SynchronizationContext, result.ExecutionContext);
			},
			(null, null, null)
		);

	/// <inheritdoc/>
	protected override string? GetRuntimeSkipReason() =>
		getRuntimeSkipReason.Value;

	/// <inheritdoc/>
	public override void RunAfter(BeforeAfterTestAttribute attribute) =>
		Guard.ArgumentNotNull(attribute).After(Test);

	/// <inheritdoc/>
	public override void RunBefore(BeforeAfterTestAttribute attribute) =>
		Guard.ArgumentNotNull(attribute).Before(Test);

	string? SafeGetRuntimeSkipReason() =>
		// We want to record any issues as exceptions in the aggregator so that the test
		// fails rather than run. We know the first time we're called it'll be before test
		// invocation, so recording the exception will result in a test failure.
		Aggregator.Run(() =>
		{
			var skipReason = Test.SkipReason;
			var skipUnless = Test.SkipUnless;
			var skipWhen = Test.SkipWhen;

			return (skipUnless, skipWhen) switch
			{
				(null, null) => skipReason,
				(not null, not null) =>
					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Both 'SkipUnless' and 'SkipWhen' are set on test method '{0}.{1}'; they are mutually exclusive",
							Test.TestCase.TestClassName,
							Test.TestCase.TestMethodName
						)
					),
				(not null, null) => skipUnless() ? null : validateSkipReason(skipReason, Test),
				(null, not null) => skipWhen() ? validateSkipReason(skipReason, Test) : null,
			};

			static string validateSkipReason(
				string? skipReason,
				ICodeGenTest test)
			{
				if (skipReason is null)
					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"You must set 'Skip' when you set 'SkipUnless' or 'SkipWhen' on test method '{0}.{1}' to set the message for conditional skips",
							test.TestCase.TestClassName,
							test.TestCase.TestMethodName
						)
					);

				return skipReason;
			}
		}, null);
}
