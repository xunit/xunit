using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestRunnerBaseContext<TTest> : CoreTestRunnerContext<TTest, IBeforeAfterTestAttribute>
	where TTest : class, IXunitTest
{
	// We want to cache the results of this, since it will potentially be called more than once,
	// and it involves reflection and dynamic invocation.
	readonly Lazy<string?> getRuntimeSkipReason;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunnerBaseContext{TTest}"/> class.
	/// </summary>
	/// <param name="test">The test</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="beforeAfterTestAttributes">The <see cref="IBeforeAfterTestAttribute"/>s that are applied to the test</param>
	/// <param name="constructorArguments">The constructor arguments for the test class</param>
	public XunitTestRunnerBaseContext(
		TTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
		object?[] constructorArguments) :
			base(Guard.ArgumentNotNull(test), messageBus, test.SkipReason, explicitOption, aggregator, cancellationTokenSource)
	{
		BeforeAfterTestAttributes = Guard.ArgumentNotNull(beforeAfterTestAttributes);
		ConstructorArguments = Guard.ArgumentNotNull(constructorArguments);

		getRuntimeSkipReason = new(SafeGetRuntimeSkipReason);
	}

	/// <inheritdoc/>
	protected override IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }

	/// <summary>
	/// Gets the arguments that should be passed to the test class when it's constructed.
	/// </summary>
	public object?[] ConstructorArguments { get; }

	/// <summary>
	/// Gets the method that this test originated in.
	/// </summary>
	public MethodInfo Method =>
		Test.TestMethod.Method;

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] MethodArguments =>
		Test.TestMethodArguments;

	/// <inheritdoc/>
	protected override string? GetRuntimeSkipReason() =>
		getRuntimeSkipReason.Value;

	/// <inheritdoc/>
	public override ValueTask<TimeSpan> InvokeTest(object? testClassInstance) =>
		ExecutionTimer.MeasureAsync(
			() => Aggregator.RunAsync(
				async () =>
				{
					var parameterCount = Method.GetParameters().Length;
					var valueCount = MethodArguments is null ? 0 : MethodArguments.Length;
					if (parameterCount != valueCount)
					{
						Aggregator.Add(
							new TestPipelineException(
								string.Format(
									CultureInfo.CurrentCulture,
									"The test method expected {0} parameter value{1}, but {2} parameter value{3} {4} provided.",
									parameterCount,
									parameterCount == 1 ? "" : "s",
									valueCount,
									valueCount == 1 ? "" : "s",
									valueCount == 1 ? "was" : "were"
								)
							)
						);
					}
					else
					{
						var result = InvokeTestMethod(testClassInstance);
						var valueTask = AsyncUtility.TryConvertToValueTask(result);
						if (valueTask.HasValue)
							await valueTask.Value;
					}
				}
			)
		);

	/// <summary>
	/// Called by <see cref="InvokeTest"/> to invoke the test method.
	/// </summary>
	/// <param name="testClassInstance">The instance of the test class (may be <see langword="null"/> when
	/// running a static test method)</param>
	/// <returns>Returns the result of the test method invocation. This value is evaluated to determine
	/// if it indicates asynchronous execution (by calling <see cref="AsyncUtility.TryConvertToValueTask"/>)
	/// so that <see cref="InvokeTest"/> can wait on the resulting operation.</returns>
	/// <remarks>
	/// By default, this uses reflection to invoke the test method from the context. Developers can
	/// override this method to replace the test method invocation and/or to add operations before/after
	/// the test method is invoked.
	/// </remarks>
	/// <inheritdoc/>
	protected virtual object? InvokeTestMethod(object? testClassInstance) =>
		Method.Invoke(testClassInstance, MethodArguments);

	/// <inheritdoc/>
	public override void RunAfter(IBeforeAfterTestAttribute attribute) =>
		Guard.ArgumentNotNull(attribute).After(Test.TestMethod.Method, Test);

	/// <inheritdoc/>
	public override void RunBefore(IBeforeAfterTestAttribute attribute) =>
		Guard.ArgumentNotNull(attribute).Before(Test.TestMethod.Method, Test);

	string? SafeGetRuntimeSkipReason() =>
		// We want to record any issues as exceptions in the aggregator so that the test
		// fails rather than run. We know the first time we're call it'll be before test
		// invocation, so recording the exception will result in a test failure.
		Aggregator.Run(() =>
		{
			var skipReason = Test.SkipReason;
			var skipUnless = Test.SkipUnless;
			var skipWhen = Test.SkipWhen;

			if (skipUnless is null && skipWhen is null)
				return skipReason;
			if (skipUnless is not null && skipWhen is not null)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Both 'SkipUnless' and 'SkipWhen' are set on test method '{0}.{1}'; they are mutually exclusive",
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);
			if (skipReason is null)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"You must set 'Skip' when you set 'SkipUnless' or 'SkipWhen' on test method '{0}.{1}' to set the message for conditional skips",
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);

			var propertyType = Test.SkipType ?? Test.TestCase.TestClass.Class;
			var propertyName = (skipUnless ?? skipWhen)!;
			var property =
				propertyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
					?? throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Cannot find public static property '{0}' on type '{1}' for dynamic skip on test method '{2}.{3}'",
							propertyName,
							propertyType,
							Test.TestCase.TestClassName,
							Test.TestCase.TestMethodName
						)
					);
			var getMethod =
				property.GetGetMethod()
					?? throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Public static property '{0}' on type '{1}' must be readable for dynamic skip on test method '{2}.{3}'",
							propertyName,
							propertyType,
							Test.TestCase.TestClassName,
							Test.TestCase.TestMethodName
						)
					);
			if (getMethod.ReturnType != typeof(bool) || getMethod.Invoke(null, []) is not bool result)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Public static property '{0}' on type '{1}' must return bool for dynamic skip on test method '{2}.{3}'",
						propertyName,
						propertyType,
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);

			var shouldSkip = (skipUnless, skipWhen, result) switch
			{
				(not null, _, false) => true,
				(_, not null, true) => true,
				_ => false,
			};

			return shouldSkip ? skipReason : null;
		}, null);
}
