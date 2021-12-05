using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class that provides default behavior when running a test. This includes support
	/// for skipping tests.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestRunner<TTestCase>
		where TTestCase : _ITestCase
	{
		CancellationTokenSource cancellationTokenSource;
		object?[] constructorArguments;
		IMessageBus messageBus;
		_ITest test;
		Type testClass;
		MethodInfo testMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="test">The test that this invocation belongs to.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testClass">The test class that the test method belongs to.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethod">The test method that will be invoked.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestRunner(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			SkipReason = skipReason;
			TestMethodArguments = testMethodArguments;
			Aggregator = aggregator;

			this.test = Guard.ArgumentNotNull(test);
			this.messageBus = Guard.ArgumentNotNull(messageBus);
			this.testClass = Guard.ArgumentNotNull(testClass);
			this.constructorArguments = Guard.ArgumentNotNull(constructorArguments);
			this.testMethod = Guard.ArgumentNotNull(testMethod);
			this.cancellationTokenSource = Guard.ArgumentNotNull(cancellationTokenSource);

			Guard.ArgumentValid($"test.TestCase must implement {typeof(TTestCase).FullName}", test.TestCase is TTestCase, nameof(test));
		}

		/// <summary>
		/// Gets or sets the exception aggregator used to run code and collect exceptions.
		/// </summary>
		protected ExceptionAggregator Aggregator { get; set; }

		/// <summary>
		/// Gets or sets the task cancellation token source, used to cancel the test run.
		/// </summary>
		protected CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => cancellationTokenSource = Guard.ArgumentNotNull(value, nameof(CancellationTokenSource));
		}

		/// <summary>
		/// Gets or sets the constructor arguments used to construct the test class.
		/// </summary>
		protected object?[] ConstructorArguments
		{
			get => constructorArguments;
			set => constructorArguments = Guard.ArgumentNotNull(value, nameof(ConstructorArguments));
		}

		/// <summary>
		/// Gets or sets the display name of the invoked test.
		/// </summary>
		protected string DisplayName => Test.DisplayName;

		/// <summary>
		/// Gets or sets the message bus to report run status to.
		/// </summary>
		protected IMessageBus MessageBus
		{
			get => messageBus;
			set => messageBus = Guard.ArgumentNotNull(value, nameof(MessageBus));
		}

		/// <summary>
		/// Gets or sets the skip reason for the test, if set.
		/// </summary>
		protected string? SkipReason { get; set; }

		/// <summary>
		/// Gets or sets the test to be run.
		/// </summary>
		protected _ITest Test
		{
			get => test;
			set => test = Guard.ArgumentNotNull(value, nameof(Test));
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
			set => testClass = Guard.ArgumentNotNull(value, nameof(TestClass));
		}

		/// <summary>
		/// Gets or sets the runtime method of the method that contains the test.
		/// </summary>
		protected MethodInfo TestMethod
		{
			get => testMethod;
			set => testMethod = Guard.ArgumentNotNull(value, nameof(TestMethod));
		}

		/// <summary>
		/// Gets or sets the arguments to pass to the test method when it's being invoked.
		/// Maybe be <c>null</c> to indicate there are no arguments.
		/// </summary>
		protected object?[]? TestMethodArguments { get; set; }

		/// <summary>
		/// This method is called just after <see cref="_TestStarting"/> is sent, but before the test class is created.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual void AfterTestStarting()
		{ }

		/// <summary>
		/// This method is called just before <see cref="_TestFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual void BeforeTestFinished()
		{ }

		/// <summary>
		/// Override this method to invoke the test.
		/// </summary>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <returns>Returns a tuple which includes the execution time (in seconds) spent running the
		/// test method, and any output that was returned by the test.</returns>
		protected abstract ValueTask<Tuple<decimal, string>?> InvokeTestAsync(ExceptionAggregator aggregator);

		/// <summary>
		/// Runs the test.
		/// </summary>
		/// <returns>Returns summary information about the test that was run.</returns>
		public async ValueTask<RunSummary> RunAsync()
		{
			SetTestContext(TestEngineStatus.Initializing);

			var runSummary = new RunSummary { Total = 1 };
			var output = string.Empty;

			var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
			var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = TestCase.UniqueID;
			var testUniqueID = Test.UniqueID;

			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestDisplayName = Test.DisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};

			if (!MessageBus.QueueMessage(testStarting))
				CancellationTokenSource.Cancel();
			else
			{
				AfterTestStarting();

				_TestResultMessage testResult;

				if (!string.IsNullOrEmpty(SkipReason))
				{
					runSummary.Skipped++;

					testResult = new _TestSkipped
					{
						AssemblyUniqueID = testAssemblyUniqueID,
						ExecutionTime = 0m,
						Output = "",
						Reason = SkipReason,
						TestCaseUniqueID = testCaseUniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID
					};
				}
				else
				{
					var aggregator = new ExceptionAggregator(Aggregator);

					if (!aggregator.HasExceptions)
					{
						var tuple = await aggregator.RunAsync(() => InvokeTestAsync(aggregator));
						if (tuple != null)
						{
							runSummary.Time = tuple.Item1;
							output = tuple.Item2;
						}
					}

					var exception = aggregator.ToException();

					if (exception == null)
					{
						testResult = new _TestPassed
						{
							AssemblyUniqueID = testAssemblyUniqueID,
							ExecutionTime = runSummary.Time,
							Output = output,
							TestCaseUniqueID = testCaseUniqueID,
							TestClassUniqueID = testClassUniqueID,
							TestCollectionUniqueID = testCollectionUniqueID,
							TestMethodUniqueID = testMethodUniqueID,
							TestUniqueID = testUniqueID
						};
					}
					// We don't want a strongly typed contract here; any exception can be a dynamically
					// skipped exception so long as its message starts with the special token.
					else if (exception.Message.StartsWith(DynamicSkipToken.Value))
					{
						testResult = new _TestSkipped
						{
							AssemblyUniqueID = testAssemblyUniqueID,
							ExecutionTime = runSummary.Time,
							Output = output,
							Reason = exception.Message.Substring(DynamicSkipToken.Value.Length),
							TestCaseUniqueID = testCaseUniqueID,
							TestClassUniqueID = testClassUniqueID,
							TestCollectionUniqueID = testCollectionUniqueID,
							TestMethodUniqueID = testMethodUniqueID,
							TestUniqueID = testUniqueID
						};
						runSummary.Skipped++;
					}
					else
					{
						testResult = _TestFailed.FromException(
							exception,
							testAssemblyUniqueID,
							testCollectionUniqueID,
							testClassUniqueID,
							testMethodUniqueID,
							testCaseUniqueID,
							testUniqueID,
							runSummary.Time,
							output
						);
						runSummary.Failed++;
					}
				}

				SetTestContext(TestEngineStatus.CleaningUp, TestState.FromTestResult(testResult));

				if (!CancellationTokenSource.IsCancellationRequested)
					if (!MessageBus.QueueMessage(testResult))
						CancellationTokenSource.Cancel();

				Aggregator.Clear();
				BeforeTestFinished();

				if (Aggregator.HasExceptions)
				{
					var testCleanupFailure = _TestCleanupFailure.FromException(
						Aggregator.ToException()!,
						testAssemblyUniqueID,
						testCollectionUniqueID,
						testClassUniqueID,
						testMethodUniqueID,
						testCaseUniqueID,
						testUniqueID
					);

					if (!MessageBus.QueueMessage(testCleanupFailure))
						CancellationTokenSource.Cancel();
				}

				var testFinished = new _TestFinished
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExecutionTime = runSummary.Time,
					Output = output,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

				if (!MessageBus.QueueMessage(testFinished))
					CancellationTokenSource.Cancel();
			}

			return runSummary;
		}

		/// <summary>
		/// Sets the test context for the given test state and engine status.
		/// </summary>
		/// <param name="testStatus">The current engine status for the test</param>
		/// <param name="testState">The current test state</param>
		protected virtual void SetTestContext(
			TestEngineStatus testStatus,
			TestState? testState = null) =>
				TestContext.SetForTest(test, testStatus, CancellationTokenSource.Token, testState);
	}
}
