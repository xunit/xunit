using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running a test. This includes support
/// for skipping tests.
/// </summary>
/// <remarks>
/// This class does not make any assumptions about what it means to run an individual test,
/// just that at some point, the test will be run. The intention with this base class is that
/// it can serve as a base for non-traditional tests.
/// </remarks>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTest">The test type used by the test framework. Must derive from
/// <see cref="ITest"/>.</typeparam>
public abstract class TestRunner<TContext, TTest> :
	TestRunnerBase<TContext, TTest>
		where TContext : TestRunnerContext<TTest>
		where TTest : class, ITest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestRunner{TContext, TTest}"/> class.
	/// </summary>
	protected TestRunner()
	{ }

	/// <inheritdoc/>
	async ValueTask<(object?, SynchronizationContext?, ExecutionContext?)> CreateTestClass(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.Aggregator.Run(() => IsTestClassCreatable(ctxt), false))
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		if (ctxt.CancellationTokenSource.IsCancellationRequested || ctxt.Aggregator.HasExceptions)
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassConstructionStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.CancellationTokenSource.IsCancellationRequested || ctxt.Aggregator.HasExceptions)
			return (null, SynchronizationContext.Current, ExecutionContext.Capture());

		var result = await ctxt.Aggregator.RunAsync(() => CreateTestClassInstance(ctxt), (null, null, null));

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassConstructionFinished(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		return result;
	}

	/// <summary>
	/// Override to creates and initialize the instance of the test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns the test class instance, the sync context that is current after the creation,
	/// and a capture of the execution context so that it can be restored later.</returns>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure. Since the method is potentially async, we depend on it to capture and
	/// return the sync context so that it may be propagated appropriately.
	/// </remarks>
	protected abstract ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TContext ctxt);

	async ValueTask DisposeTestClass(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (testClassInstance is null || !ctxt.Aggregator.Run(() => IsTestClassDisposable(ctxt, testClassInstance), false))
			return;

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassDisposeStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		await ctxt.Aggregator.RunAsync(() => DisposeTestClassInstance(ctxt, testClassInstance));

		if (!await ctxt.Aggregator.RunAsync(() => OnTestClassDisposeFinished(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();
	}

	/// <summary>
	/// Disposes the test class instance. By default, will call <see cref="IAsyncDisposable.DisposeAsync"/> if
	/// it's implemented, falling back to <see cref="IDisposable.Dispose"/> if not.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	protected virtual async ValueTask DisposeTestClassInstance(
		TContext ctxt,
		object testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (testClassInstance is IAsyncDisposable asyncDisposable)
			await asyncDisposable.DisposeAsync();
		else if (testClassInstance is IDisposable disposable)
			disposable.Dispose();
	}

	/// <summary>
	/// Invokes the test method and returns the amount of time spent executing.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The instance of the test class (may be <c>null</c> when
	/// running a static test method)</param>
	/// <returns>Returns the execution time (in seconds) spent running the test method.</returns>
	protected virtual ValueTask<TimeSpan> InvokeTest(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Test.TestCase.TestMethod is null)
		{
			ctxt.Aggregator.Add(
				new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Test '{0}' does not have an associated method and cannot be run by TestRunner",
						ctxt.Test.TestDisplayName
					)
				)
			);

			return new(TimeSpan.Zero);
		}

		return ExecutionTimer.MeasureAsync(
			() => ctxt.Aggregator.RunAsync(
				async () =>
				{
					var parameterCount = ctxt.TestMethod.GetParameters().Length;
					var valueCount = ctxt.TestMethodArguments is null ? 0 : ctxt.TestMethodArguments.Length;
					if (parameterCount != valueCount)
					{
						ctxt.Aggregator.Add(
							new InvalidOperationException(
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
						var logEnabled = TestEventSource.Log.IsEnabled();

						if (logEnabled)
							TestEventSource.Log.TestStart(ctxt.Test.TestDisplayName);

						try
						{
							var result = ctxt.TestMethod.Invoke(testClassInstance, ctxt.TestMethodArguments);
							var valueTask = AsyncUtility.TryConvertToValueTask(result);
							if (valueTask.HasValue)
								await valueTask.Value;
						}
						finally
						{
							if (logEnabled)
								TestEventSource.Log.TestStop(ctxt.Test.TestDisplayName);
						}
					}
				}
			)
		);
	}

	/// <summary>
	/// Override to determine whether a test class should be created.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure (and test class creation will not take place).
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected abstract bool IsTestClassCreatable(TContext ctxt);

	/// <summary>
	/// Determine whether a test class instance should be disposed. The pipeline will only call
	/// <see cref="DisposeTestClassInstance"/> if this returns <c>true</c>. By default, looks to
	/// see if the class implements <see cref="IAsyncDisposable"/> or <see cref="IDisposable"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	protected virtual bool IsTestClassDisposable(
		TContext ctxt,
		object testClassInstance) =>
			testClassInstance is IDisposable or IAsyncDisposable;

	/// <summary>
	/// This method will be called when a test class instance has finished being constructed. By
	/// default, this sends <see cref="TestClassConstructionFinished"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassConstructionFinished(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassConstructionFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when a test class instance is about to be constructed. By
	/// default, this sends <see cref="TestClassConstructionStarting"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure (and test class creation will not take place).
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassConstructionStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassConstructionStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when a test class instance has finished being disposed. By
	/// default, this sends <see cref="TestClassDisposeFinished"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassDisposeFinished(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassDisposeFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when a test class instance is about to be disposed. By
	/// default, this sends <see cref="TestClassDisposeStarting"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestClassDisposeStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestClassDisposeStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <summary>
	/// Override this method to call code just after the test invocation has completed, but before
	/// the test class instance has been disposed.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual void PostInvoke(TContext ctxt)
	{ }

	/// <summary>
	/// Override this method to call code just after the test class instance has been created, but
	/// before the test has been invoked.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual void PreInvoke(TContext ctxt)
	{ }

	/// <inheritdoc/>
	protected override async ValueTask<TimeSpan> RunTest(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		object? testClassInstance = null;
		var elapsedTime = TimeSpan.Zero;

		if (!ctxt.Aggregator.HasExceptions)
		{
			SynchronizationContext? syncContext = null;
			ExecutionContext? executionContext = null;

			elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(async () => { (testClassInstance, syncContext, executionContext) = await CreateTestClass(ctxt); }));

			TaskCompletionSource<object?> finished = new();

			if (executionContext is not null)
				ExecutionContext.Run(executionContext, runTest, null);
			else
				runTest(null);

			await finished.Task;

			async void runTest(object? _)
			{
				SynchronizationContext.SetSynchronizationContext(syncContext);
				UpdateTestContext(testClassInstance);

				try
				{
					if (!ctxt.Aggregator.HasExceptions)
					{
						elapsedTime += ExecutionTimer.Measure(() => ctxt.Aggregator.Run(() => PreInvoke(ctxt)));

						if (!ctxt.Aggregator.HasExceptions)
						{
							elapsedTime += await ctxt.Aggregator.RunAsync(() => InvokeTest(ctxt, testClassInstance), TimeSpan.Zero);

							// Set an early version of TestResultState so anything done in PostInvoke can understand whether
							// it looks like the test is passing, failing, or dynamically skipped
							var currentException = ctxt.Aggregator.ToException();
							var currentSkipReason = ctxt.GetSkipReason(currentException);
							var currentExecutionTime = (decimal)elapsedTime.TotalMilliseconds;
							var testResultState =
								currentSkipReason is not null
									? TestResultState.ForSkipped(currentExecutionTime)
									: TestResultState.FromException(currentExecutionTime, currentException);

							UpdateTestContext(testClassInstance, testResultState);

							elapsedTime += ExecutionTimer.Measure(() => ctxt.Aggregator.Run(() => PostInvoke(ctxt)));
						}

						elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(() => DisposeTestClass(ctxt, testClassInstance)));

						UpdateTestContext(null, TestContext.Current.TestState);
					}

					finished.TrySetResult(null);
				}
				catch (Exception ex)
				{
					finished.TrySetException(ex);
				}
			}
		}

		return elapsedTime;
	}
}
