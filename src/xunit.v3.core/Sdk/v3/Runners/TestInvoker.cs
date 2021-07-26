﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class that provides default behavior to invoke a test method. This includes
	/// support for async test methods (both "async Task" and "async void") as well as
	/// creation and disposal of the test class.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestInvoker<TTestCase>
		where TTestCase : _ITestCase
	{
		static MethodInfo? fSharpStartAsTaskOpenGenericMethod;

		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		object?[] constructorArguments;
		IMessageBus messageBus;
		_ITest test;
		Type testClass;
		MethodInfo testMethod;
		ExecutionTimer timer = new ExecutionTimer();

		/// <summary>
		/// Initializes a new instance of the <see cref="TestInvoker{TTestCase}"/> class.
		/// </summary>
		/// <param name="test">The test that this invocation belongs to.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testClass">The test class that the test method belongs to.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethod">The test method that will be invoked.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestInvoker(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			this.test = Guard.ArgumentNotNull(nameof(test), test);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.testClass = Guard.ArgumentNotNull(nameof(testClass), testClass);
			this.constructorArguments = Guard.ArgumentNotNull(nameof(constructorArguments), constructorArguments);
			this.testMethod = Guard.ArgumentNotNull(nameof(testMethod), testMethod);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);

			TestMethodArguments = testMethodArguments;

			Guard.ArgumentValid("test", $"test.TestCase must implement {typeof(TTestCase).FullName}", test.TestCase is TTestCase);
		}

		/// <summary>
		/// Gets or sets the exception aggregator used to run code and collect exceptions.
		/// </summary>
		protected ExceptionAggregator Aggregator
		{
			get => aggregator;
			set => aggregator = Guard.ArgumentNotNull(nameof(Aggregator), value);
		}

		/// <summary>
		/// Gets or sets the task cancellation token source, used to cancel the test run.
		/// </summary>
		protected CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => cancellationTokenSource = Guard.ArgumentNotNull(nameof(CancellationTokenSource), value);
		}

		/// <summary>
		/// Gets or sets the constructor arguments used to construct the test class.
		/// </summary>
		protected object?[] ConstructorArguments
		{
			get => constructorArguments;
			set => constructorArguments = Guard.ArgumentNotNull(nameof(ConstructorArguments), value);
		}

		/// <summary>
		/// Gets the display name of the invoked test.
		/// </summary>
		protected string DisplayName => Test.DisplayName;

		/// <summary>
		/// Gets or sets the message bus to report run status to.
		/// </summary>
		protected IMessageBus MessageBus
		{
			get => messageBus;
			set => messageBus = Guard.ArgumentNotNull(nameof(MessageBus), value);
		}

		/// <summary>
		/// Gets or sets the test to be run.
		/// </summary>
		protected _ITest Test
		{
			get => test;
			set => test = Guard.ArgumentNotNull(nameof(Test), value);
		}

		/// <summary>
		/// Gets the test case to be run.
		/// </summary>
		protected TTestCase TestCase => (TTestCase)Test.TestCase;

		/// <summary>
		/// Gets or sets the runtime type of the class that contains the test method.
		/// </summary>
		protected Type TestClass
		{
			get => testClass;
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <summary>
		/// Gets or sets the runtime method of the method that contains the test.
		/// </summary>
		protected MethodInfo TestMethod
		{
			get => testMethod;
			set => testMethod = Guard.ArgumentNotNull(nameof(TestMethod), value);
		}

		/// <summary>
		/// Gets or sets the arguments to pass to the test method when it's being invoked.
		/// </summary>
		protected object?[]? TestMethodArguments { get; set; }

		/// <summary>
		/// Gets or sets the object which measures execution time.
		/// </summary>
		protected ExecutionTimer Timer
		{
			get => timer;
			set => timer = Guard.ArgumentNotNull(nameof(Timer), value);
		}

		/// <summary>
		/// Creates the test class, unless the test method is static or there have already been errors. Note that
		/// this method times the creation of the test class (using <see cref="Timer"/>). It is also responsible for
		/// sending the <see cref="_TestClassConstructionStarting"/> and <see cref="_TestClassConstructionFinished"/>
		/// messages, so if you override this method without calling the base, you are responsible for all of this behavior.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>. To override
		/// just the behavior of creating the instance of the test class, override <see cref="CreateTestClassInstance"/>
		/// instead.
		/// </summary>
		/// <returns>The class instance, if appropriate; <c>null</c>, otherwise</returns>
		protected virtual object? CreateTestClass()
		{
			object? testClass = null;

			if (!TestMethod.IsStatic && !Aggregator.HasExceptions)
			{
				var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
				var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
				var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
				var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
				var testCaseUniqueID = TestCase.UniqueID;
				var testUniqueID = Test.UniqueID;

				var testClassConstructionStarting = new _TestClassConstructionStarting
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

				if (!messageBus.QueueMessage(testClassConstructionStarting))
					cancellationTokenSource.Cancel();
				else
				{
					try
					{
						if (!cancellationTokenSource.IsCancellationRequested)
							Timer.Aggregate(() => testClass = CreateTestClassInstance());
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
						if (!messageBus.QueueMessage(testClassConstructionFinished))
							cancellationTokenSource.Cancel();
					}
				}
			}

			return testClass;
		}

		/// <summary>
		/// Creates the instance of the test class. By default, uses <see cref="Activator"/>.<see cref="Activator.CreateInstance(Type, object[])"/>
		/// with the values from <see cref="TestClass"/> and <see cref="ConstructorArguments"/>. You should override this in order to
		/// change the input values and/or use a factory method other than Activator.CreateInstance.
		/// </summary>
		/// <returns></returns>
		protected virtual object? CreateTestClassInstance() =>
			Activator.CreateInstance(TestClass, ConstructorArguments);

		/// <summary>
		/// This method is called just after the test method has finished executing.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task AfterTestMethodInvokedAsync() => Task.CompletedTask;

		/// <summary>
		/// This method is called just before the test method is invoked.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task BeforeTestMethodInvokedAsync() => Task.CompletedTask;

		/// <summary>
		/// This method calls the test method via reflection. This is an available override point
		/// if you need to do some other form of invocation of the actual test method.
		/// </summary>
		/// <param name="testClassInstance">The instance of the test class</param>
		/// <returns>The return value from the test method invocation</returns>
		protected virtual object? CallTestMethod(object? testClassInstance) =>
			TestMethod.Invoke(testClassInstance, TestMethodArguments);

		/// <summary>
		/// This method is obsolete. Call <see cref="GetValueTaskFromResult(object?)"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This method has been removed in favor of GetValueTaskFromResult", true)]
		public static Task? GetTaskFromResult(object? obj) =>
			throw new NotImplementedException();

		/// <summary>
		/// Given an object, will attempt to convert instances of <see cref="Task"/> or
		/// <see cref="T:Microsoft.FSharp.Control.FSharpAsync`1"/> into <see cref="ValueTask"/>
		/// as appropriate.
		/// </summary>
		/// <param name="obj">The object to convert</param>
		public static ValueTask? GetValueTaskFromResult(object? obj)
		{
			if (obj == null)
				return null;

			if (obj is Task task)
			{
				if (task.Status == TaskStatus.Created)
					throw new InvalidOperationException("Test method returned a non-started Task (tasks must be started before being returned)");

				return new ValueTask(task);
			}

			if (obj is ValueTask valueTask)
				return valueTask;

			var type = obj.GetType();
			if (type.IsGenericType && type.GetGenericTypeDefinition().FullName == "Microsoft.FSharp.Control.FSharpAsync`1")
			{
				if (fSharpStartAsTaskOpenGenericMethod == null)
				{
					fSharpStartAsTaskOpenGenericMethod =
						type
							.Assembly
							.GetType("Microsoft.FSharp.Control.FSharpAsync")?
							.GetRuntimeMethods()
							.FirstOrDefault(m => m.Name == "StartAsTask");

					if (fSharpStartAsTaskOpenGenericMethod == null)
						throw new InvalidOperationException("Test returned an F# async result, but could not find 'Microsoft.FSharp.Control.FSharpAsync.StartAsTask'");
				}

				var fsharpTask =
					fSharpStartAsTaskOpenGenericMethod
						.MakeGenericMethod(type.GetGenericArguments()[0])
						.Invoke(null, new[] { obj, null, null }) as Task;

				if (fsharpTask != null)
					return new ValueTask(fsharpTask);
			}

			return null;
		}

		/// <summary>
		/// Creates the test class (if necessary), and invokes the test method.
		/// </summary>
		/// <returns>Returns the time (in seconds) spent creating the test class, running
		/// the test, and disposing of the test class.</returns>
		public Task<decimal> RunAsync()
		{
			return Aggregator.RunAsync(async () =>
			{
				if (!CancellationTokenSource.IsCancellationRequested)
				{
					SetTestContext(TestEngineStatus.Initializing);

					object? testClassInstance = null;
					Timer.Aggregate(() => testClassInstance = CreateTestClass());

					var asyncDisposable = testClassInstance as IAsyncDisposable;
					var disposable = testClassInstance as IDisposable;

					var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
					var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
					var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
					var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
					var testCaseUniqueID = TestCase.UniqueID;
					var testUniqueID = Test.UniqueID;

					try
					{
						if (testClassInstance is IAsyncLifetime asyncLifetime)
							await Timer.AggregateAsync(asyncLifetime.InitializeAsync);

						try
						{
							if (!CancellationTokenSource.IsCancellationRequested)
							{
								await BeforeTestMethodInvokedAsync();

								SetTestContext(TestEngineStatus.Running);

								if (!CancellationTokenSource.IsCancellationRequested && !Aggregator.HasExceptions)
									await InvokeTestMethodAsync(testClassInstance);

								SetTestContext(TestEngineStatus.CleaningUp, TestState.FromException(Timer.Total, Aggregator.ToException()));

								await AfterTestMethodInvokedAsync();
							}
						}
						finally
						{
							if (asyncDisposable != null || disposable != null)
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

								if (!messageBus.QueueMessage(testClassDisposeStarting))
									cancellationTokenSource.Cancel();
							}

							if (asyncDisposable != null)
								await Aggregator.RunAsync(() => Timer.AggregateAsync(asyncDisposable.DisposeAsync));
						}
					}
					finally
					{
						if (disposable != null)
							Aggregator.Run(() => Timer.Aggregate(disposable.Dispose));

						if (asyncDisposable != null || disposable != null)
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

							if (!messageBus.QueueMessage(testClassDisposeFinished))
								cancellationTokenSource.Cancel();
						}
					}
				}

				return Timer.Total;
			});
		}

		/// <summary>
		/// Invokes the test method on the given test class instance. This method sets up support for "async void"
		/// test methods, ensures that the test method has the correct number of arguments, then calls <see cref="CallTestMethod"/>
		/// to do the actual method invocation. It ensure that any async test method is fully completed before returning, and
		/// returns the measured clock time that the invocation took. This method should NEVER throw; any exceptions should be
		/// placed into the <see cref="Aggregator"/>. Time spent executing user code should be measured with one of the
		/// Aggregate methods on <see cref="Timer"/>.
		/// </summary>
		/// <param name="testClassInstance">The test class instance</param>
		protected virtual async Task InvokeTestMethodAsync(object? testClassInstance)
		{
			var oldSyncContext = SynchronizationContext.Current;

			try
			{
				var asyncSyncContext = new AsyncTestSyncContext(oldSyncContext);
				SetSynchronizationContext(asyncSyncContext);

				await Aggregator.RunAsync(
					() => Timer.AggregateAsync(
						async () =>
						{
							var parameterCount = TestMethod.GetParameters().Length;
							var valueCount = TestMethodArguments == null ? 0 : TestMethodArguments.Length;
							if (parameterCount != valueCount)
							{
								Aggregator.Add(
									new InvalidOperationException(
										$"The test method expected {parameterCount} parameter value{(parameterCount == 1 ? "" : "s")}, but {valueCount} parameter value{(valueCount == 1 ? "" : "s")} {(valueCount == 1 ? "was" : "were")} provided."
									)
								);
							}
							else
							{
								var result = CallTestMethod(testClassInstance);
								var valueTask = GetValueTaskFromResult(result);
								if (valueTask.HasValue)
									await valueTask.Value;
								else
								{
									var ex = await asyncSyncContext.WaitForCompletionAsync();
									if (ex != null)
										Aggregator.Add(ex);
								}
							}
						}
					)
				);
			}
			finally
			{
				SetSynchronizationContext(oldSyncContext);
			}
		}

		[SecuritySafeCritical]
		static void SetSynchronizationContext(SynchronizationContext? context) =>
			SynchronizationContext.SetSynchronizationContext(context);

		/// <summary>
		/// Sets the test context for the given test state and engine status.
		/// </summary>
		/// <param name="testStatus">The current engine status for the test</param>
		/// <param name="testState">The current test state</param>
		protected virtual void SetTestContext(
			TestEngineStatus testStatus,
			TestState? testState = null) =>
				TestContext.SetForTest(Test, testStatus, testState);
	}
}
