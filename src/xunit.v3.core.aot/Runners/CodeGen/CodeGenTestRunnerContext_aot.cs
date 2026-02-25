using Xunit.Sdk;

namespace Xunit.v3;

// TODO: Should we put an extra generic on CoreTestRunnerContext for the type of BeforeAfter and move the majority
// of the implementation for RunBeforeAttributes/RunAfterAttributes into Core?

/// <summary>
/// Context class for <see cref="CodeGenTestRunner"/>.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestRunnerContext : CoreTestRunnerContext<ICodeGenTest, BeforeAfterTestAttribute>
{
	// We want to cache the results of this, since it will potentially be called more than once,
	// and it involves invoking lambdas.
	readonly Lazy<string?> getRuntimeSkipReason;

	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestRunnerContext"/> class.
	/// </summary>
	/// <param name="test">The test</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
	public CodeGenTestRunnerContext(
		ICodeGenTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager classFixtureMappings) :
			base(Guard.ArgumentNotNull(test), messageBus, test.SkipReason, explicitOption, aggregator, cancellationTokenSource)
	{
		BeforeAfterTestAttributes = Test.TestCase.TestMethod.BeforeAfterTestAttributes;
		ClassFixtureMappings = Guard.ArgumentNotNull(classFixtureMappings);

		getRuntimeSkipReason = new(SafeGetRuntimeSkipReason);
	}

	/// <inheritdoc/>
	protected override IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }

	/// <summary>
	/// Gets the class fixture mappings.
	/// </summary>
	public FixtureMappingManager ClassFixtureMappings { get; }

	/// <summary>
	/// Creates the test class instance.
	/// </summary>
	public ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance() =>
		Aggregator.RunAsync(
			async () =>
			{
				var result = await Test.TestCase.TestClass.TestClassFactory(ClassFixtureMappings);

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
	public override ValueTask<TimeSpan> InvokeTest(object? testClassInstance) =>
		ExecutionTimer.MeasureAsync(() => Test.MethodInvoker(testClassInstance));

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
