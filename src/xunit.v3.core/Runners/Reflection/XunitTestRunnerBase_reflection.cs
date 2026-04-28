using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestRunnerBase<TContext, TTest> : CoreTestRunner<TContext, TTest, IBeforeAfterTestAttribute>
	where TContext : XunitTestRunnerBaseContext<TTest>
	where TTest : class, IXunitTest
{
	/// <inheritdoc/>
	protected override async ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var @class = ctxt.Test.TestMethod.TestClass.Class;
		var ctor = @class.GetConstructors().Where(ci => !ci.IsStatic && ci.IsPublic).Single();

		// We allow for Func<T> when the argument is T, such that we should be able to get the value just before
		// invoking the test. So we need to do a transform on the arguments.
		object?[]? actualCtorArguments = null;

		if (ctxt.ConstructorArguments is not null)
		{
			var ctorParams = ctor.GetParameters();

			actualCtorArguments = new object?[ctxt.ConstructorArguments.Length];

			for (var idx = 0; idx < ctxt.ConstructorArguments.Length; ++idx)
			{
				actualCtorArguments[idx] = ctxt.ConstructorArguments[idx];

				var ctorArgumentValueType = ctxt.ConstructorArguments[idx]?.GetType();
				if (ctorArgumentValueType is not null)
				{
					var ctorArgumentParamType = ctorParams[idx].ParameterType;
					if (ctorArgumentParamType != ctorArgumentValueType &&
						ctorArgumentValueType == typeof(Func<>).MakeGenericType(ctorArgumentParamType))
					{
						var invokeMethod = ctorArgumentValueType.GetMethod("Invoke", []);
						if (invokeMethod is not null)
							actualCtorArguments[idx] = invokeMethod.Invoke(ctxt.ConstructorArguments[idx], []);
					}
				}
			}
		}

		var instance = TypeActivator.Current.CreateInstance(
			ctor,
			actualCtorArguments,
			(_, missingArguments) =>
				string.Format(
					CultureInfo.CurrentCulture,
					"The following constructor parameters did not have matching fixture data: {0}",
					string.Join(", ", missingArguments.Select(a => a.ParameterType.Name + " " + a.Name))
				)
		);

		if (instance is IAsyncLifetime asyncLifetime)
			await asyncLifetime.InitializeAsync();

		return (instance, SynchronizationContext.Current, ExecutionContext.Capture());
	}

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> InvokeTest(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (AsyncUtility.IsAsyncVoid(ctxt.Method))
		{
			ctxt.Aggregator.Add(new TestPipelineException("Tests marked as 'async void' are no longer supported. Please convert to 'async Task' or 'async ValueTask'."));
			return new(TimeSpan.Zero);
		}

		return base.InvokeTest(ctxt, testClassInstance);
	}

	/// <inheritdoc/>
	protected override bool IsTestClassCreatable(TContext ctxt) =>
		!Guard.ArgumentNotNull(ctxt).Test.TestMethod.Method.IsStatic;

	/// <inheritdoc/>
	protected override async ValueTask<TimeSpan> RunTest(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		await using var lifecycleTracker = new NotificationTracker<INotifyTestLifecycle>(
			ctxt.TestFixtureMappings.ForNotification<INotifyTestLifecycle>(),
			fixture => fixture.OnTestStarting(ctxt.Test),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestFinished(ctxt.Test)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTracker<INotifyTestLifecycleAsync>(
			ctxt.TestFixtureMappings.ForNotification<INotifyTestLifecycleAsync>(),
			fixture => fixture.OnTestStartingAsync(ctxt.Test),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestFinishedAsync(ctxt.Test)),
			ctxt.CancellationTokenSource.Token
		);

		ctxt.Aggregator.Aggregate(await lifecycleTracker.Up());

		if (!ctxt.Aggregator.HasExceptions)
			ctxt.Aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		if (!ctxt.Aggregator.HasExceptions)
			return await base.RunTest(ctxt);

		return TimeSpan.Zero;
	}
}
