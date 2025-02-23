using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The base test runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public class XunitTestRunnerBase<TContext, TTest> : TestRunner<TContext, TTest>
	where TContext : XunitTestRunnerBaseContext<TTest>
	where TTest : class, IXunitTest
{
	/// <inheritdoc/>
	protected override async ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var @class = ctxt.Test.TestMethod.TestClass.Class;

		// We allow for Func<T> when the argument is T, such that we should be able to get the value just before
		// invoking the test. So we need to do a transform on the arguments.
		object?[]? actualCtorArguments = null;

		if (ctxt.ConstructorArguments is not null)
		{
			var ctorParams =
				@class
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.Single()
					.GetParameters();

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

		var instance = Activator.CreateInstance(@class, actualCtorArguments);
		if (instance is IAsyncLifetime asyncLifetime)
			await asyncLifetime.InitializeAsync();

		return (instance, SynchronizationContext.Current, ExecutionContext.Capture());
	}

	/// <inheritdoc/>
	protected override ValueTask<IReadOnlyDictionary<string, TestAttachment>?> GetAttachments(TContext ctxt) =>
		new(TestContext.Current.Attachments);

	/// <inheritdoc/>
	protected override ValueTask<string> GetTestOutput(TContext ctxt) =>
		new(TestContext.Current.TestOutputHelper?.Output ?? string.Empty);

	/// <inheritdoc/>
	protected override ValueTask<string[]?> GetWarnings(TContext ctxt) =>
		new(TestContext.Current.Warnings?.ToArray());

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> InvokeTest(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (AsyncUtility.IsAsyncVoid(ctxt.TestMethod))
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
	protected override void PostInvoke(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunAfterAttributes();

	/// <inheritdoc/>
	protected override void PreInvoke(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunBeforeAttributes();

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return OnTestStarting(ctxt, ctxt.Test.Explicit, ctxt.Test.Timeout);
	}

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> RunTest(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return
			ctxt.Test.Timeout > 0
				? RunTestWithTimeout(ctxt, ctxt.Test.Timeout)
				: base.RunTest(ctxt);
	}

	async ValueTask<TimeSpan> RunTestWithTimeout(
		TContext ctxt,
		int timeout)
	{
		// We keep our own notion of execution time, since the time recorded by
		// calling the base RunTest isn't sufficient
		var stopwatch = Stopwatch.StartNew();

		await ctxt.Aggregator.RunAsync(async () =>
		{
			var syncContext = SynchronizationContext.Current;

			Task baseTask =
				syncContext is null
					? Task.Run(async () => await base.RunTest(ctxt))
					: Task.Run(() =>
					{
						var tcs = new TaskCompletionSource<object?>();

						syncContext.Post(async _ =>
						{
							// base.RunTest is guarded against throwing, so no need to
							// try/catch and report exceptions via the TCS
							await base.RunTest(ctxt);
							tcs.TrySetResult(null);
						}, null);

						return tcs.Task;
					});

			var resultTask = await Task.WhenAny(baseTask, Task.Delay(timeout));

			if (resultTask != baseTask)
			{
				try
				{
					throw TestTimeoutException.ForTimedOutTest(timeout);
				}
				finally
				{
					TestContext.Current.CancelCurrentTest();
				}
			}
		});

		return stopwatch.Elapsed;
	}

	/// <inheritdoc/>
	protected override bool ShouldTestRun(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).ExplicitOption switch
		{
			ExplicitOption.Only => ctxt.Test.Explicit,
			ExplicitOption.Off => !ctxt.Test.Explicit,
			_ => true,
		};
}
