using System;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior to invoke a test method. This includes
/// support for async test methods ("async Task", "async ValueTask", and "async void" for C#/VB,
/// and async functions in F#) as well as creation and disposal of the test class. This class
/// is designed to be a singleton for performance reasons.
/// </summary>
/// <typeparam name="TContext">The context type used by the invoker</typeparam>
public abstract class TestInvoker<TContext>
	where TContext : TestInvokerContext
{
	/// <summary>
	/// This method is called just after the test method has finished executing.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask AfterTestMethodInvokedAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// This method is called just before the test method is invoked.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask BeforeTestMethodInvokedAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// This method calls the test method via <see cref="MethodBase.Invoke(object, object[])"/>. This is an available override
	/// point if you need to do some other form of invocation of the actual test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	/// <returns>The return value from the test method invocation</returns>
	protected virtual object? CallTestMethod(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.TestMethod.Invoke(testClassInstance, ctxt.TestMethodArguments);
	}

	/// <summary>
	/// Creates the test class, unless the test method is static or there have already been errors. Note that
	/// this method times the creation of the test class (using <see cref="Timer"/>). It is also responsible for
	/// sending the <see cref="_TestClassConstructionStarting"/> and <see cref="_TestClassConstructionFinished"/>
	/// messages, so if you override this method without calling the base, you are responsible for all of this behavior.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator inside <paramref name="ctxt"/>.
	/// To override just the behavior of creating the instance of the test class,
	/// override <see cref="CreateTestClassInstance"/> instead.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>A tuple which includes the class instance (<c>null</c> if one was not created) as well as the elapsed
	/// time spent creating the class</returns>
	protected virtual object? CreateTestClass(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.Aggregator.Run(() =>
		{
			if (!ctxt.TestMethod.IsStatic && !ctxt.Aggregator.HasExceptions)
			{
				var testAssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
				var testCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
				var testClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID;
				var testMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
				var testCaseUniqueID = ctxt.Test.TestCase.UniqueID;
				var testUniqueID = ctxt.Test.UniqueID;

				var testClassConstructionStarting = new _TestClassConstructionStarting
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

				if (!ctxt.MessageBus.QueueMessage(testClassConstructionStarting))
					ctxt.CancellationTokenSource.Cancel();
				else
				{
					try
					{
						if (!ctxt.CancellationTokenSource.IsCancellationRequested)
							return ctxt.Aggregator.Run(() => CreateTestClassInstance(ctxt), null);
					}
					finally
					{
						var testClassConstructionFinished = new _TestClassConstructionFinished
						{
							AssemblyUniqueID = testAssemblyUniqueID,
							TestCaseUniqueID = testCaseUniqueID,
							TestClassUniqueID = testClassUniqueID,
							TestCollectionUniqueID = testCollectionUniqueID,
							TestMethodUniqueID = testMethodUniqueID,
							TestUniqueID = testUniqueID
						};

						if (!ctxt.MessageBus.QueueMessage(testClassConstructionFinished))
							ctxt.CancellationTokenSource.Cancel();
					}
				}
			}

			return null;
		}, null);
	}

	/// <summary>
	/// Creates the instance of the test class. By default, uses <see cref="Activator.CreateInstance(Type, object[])"/>
	/// with the <see cref="TestInvokerContext.TestClass"/> and <see cref="TestInvokerContext.ConstructorArguments"/> values
	/// from <paramref name="ctxt"/>. You should override this in order to change the input values and/or use a factory
	/// method other than Activator.CreateInstance.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual object? CreateTestClassInstance(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return Activator.CreateInstance(ctxt.TestClass, ctxt.ConstructorArguments);
	}

	/// <summary>
	/// Invokes the test method on the given test class instance. This method sets up support for "async void"
	/// test methods, ensures that the test method has the correct number of arguments, then calls <see cref="CallTestMethod"/>
	/// to do the actual method invocation. It ensure that any async test method is fully completed before returning, and
	/// returns the measured clock time that the invocation took. This method should NEVER throw; any exceptions should be
	/// placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testClassInstance">The test class instance</param>
	/// <returns>Returns the amount of time the test took to run, in seconds</returns>
	protected virtual async ValueTask<decimal> InvokeTestMethodAsync(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		var oldSyncContext = default(SynchronizationContext);
		var asyncSyncContext = default(AsyncTestSyncContext);

		try
		{
			if (AsyncUtility.IsAsyncVoid(ctxt.TestMethod))
			{
				oldSyncContext = SynchronizationContext.Current;
				asyncSyncContext = new AsyncTestSyncContext(oldSyncContext);
				SetSynchronizationContext(asyncSyncContext);
			}

			var elapsed = await ExecutionTimer.MeasureAsync(
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
							var result = CallTestMethod(ctxt, testClassInstance);
							var valueTask = AsyncUtility.TryConvertToValueTask(result);
							if (valueTask.HasValue)
								await valueTask.Value;
							else if (asyncSyncContext is not null)
							{
								var ex = await asyncSyncContext.WaitForCompletionAsync();
								if (ex is not null)
									ctxt.Aggregator.Add(ex);
							}
						}
					}
				)
			);

			return (decimal)elapsed.TotalSeconds;
		}
		finally
		{
			if (asyncSyncContext is not null)
				SetSynchronizationContext(oldSyncContext);
		}
	}

	/// <summary>
	/// Creates the test class (if necessary), and invokes the test method. It is assumed the context is fresh, so it is
	/// initialized and disposed in this method, so disposing it elsewhere is unnecessary.
	/// </summary>
	/// <param name="ctxt">The invoker context</param>
	/// <returns>Returns the time (in seconds) spent creating the test class, running
	/// the test, and disposing of the test class.</returns>
	protected async ValueTask<decimal> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return await ctxt.Aggregator.RunAsync(async () =>
		{
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				return 0m;

			SetTestContext(ctxt, TestEngineStatus.Initializing);

			object? testClassInstance = null;
			var elapsedTime = ExecutionTimer.Measure(() => { testClassInstance = CreateTestClass(ctxt); });

			var asyncDisposable = testClassInstance as IAsyncDisposable;
			var disposable = testClassInstance as IDisposable;

			var testAssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID;
			var testMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = ctxt.Test.TestCase.UniqueID;
			var testUniqueID = ctxt.Test.UniqueID;

			try
			{
				if (testClassInstance is IAsyncLifetime asyncLifetime)
					elapsedTime += await ExecutionTimer.MeasureAsync(asyncLifetime.InitializeAsync);

				try
				{
					if (!ctxt.CancellationTokenSource.IsCancellationRequested)
					{
						elapsedTime += await ExecutionTimer.MeasureAsync(() => BeforeTestMethodInvokedAsync(ctxt));

						SetTestContext(ctxt, TestEngineStatus.Running);

						if (!ctxt.CancellationTokenSource.IsCancellationRequested && !ctxt.Aggregator.HasExceptions)
							elapsedTime += TimeSpan.FromSeconds((double)await InvokeTestMethodAsync(ctxt, testClassInstance));

						SetTestContext(ctxt, TestEngineStatus.CleaningUp, TestResultState.FromException((decimal)elapsedTime.TotalSeconds, ctxt.Aggregator.ToException()));

						elapsedTime += await ExecutionTimer.MeasureAsync(() => AfterTestMethodInvokedAsync(ctxt));
					}
				}
				finally
				{
					if (asyncDisposable is not null || disposable is not null)
					{
						var testClassDisposeStarting = new _TestClassDisposeStarting
						{
							AssemblyUniqueID = testAssemblyUniqueID,
							TestCaseUniqueID = testCaseUniqueID,
							TestClassUniqueID = testClassUniqueID,
							TestCollectionUniqueID = testCollectionUniqueID,
							TestMethodUniqueID = testMethodUniqueID,
							TestUniqueID = testUniqueID
						};

						if (!ctxt.MessageBus.QueueMessage(testClassDisposeStarting))
							ctxt.CancellationTokenSource.Cancel();
					}

					if (asyncDisposable is not null)
						elapsedTime += await ExecutionTimer.MeasureAsync(() => ctxt.Aggregator.RunAsync(asyncDisposable.DisposeAsync));
				}
			}
			finally
			{
				if (disposable is not null)
					elapsedTime += ExecutionTimer.Measure(() => ctxt.Aggregator.Run(disposable.Dispose));

				if (asyncDisposable is not null || disposable is not null)
				{
					var testClassDisposeFinished = new _TestClassDisposeFinished
					{
						AssemblyUniqueID = testAssemblyUniqueID,
						TestCaseUniqueID = testCaseUniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID
					};

					if (!ctxt.MessageBus.QueueMessage(testClassDisposeFinished))
						ctxt.CancellationTokenSource.Cancel();
				}
			}

			return (decimal)elapsedTime.TotalSeconds;
		}, 0m);
	}

	[SecuritySafeCritical]
	static void SetSynchronizationContext(SynchronizationContext? context) =>
		SynchronizationContext.SetSynchronizationContext(context);

	/// <summary>
	/// Sets the test context for the given test state and engine status.
	/// </summary>
	/// <param name="ctxt">The invoker context</param>
	/// <param name="testStatus">The current engine status for the test</param>
	/// <param name="testState">The current test state</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(ctxt.Test, testStatus, ctxt.CancellationTokenSource.Token, testState);
	}
}
