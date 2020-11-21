using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// A base class that provides default behavior when running a test. This includes support
	/// for skipping tests.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="ITestCase"/>.</typeparam>
	public abstract class TestRunner<TTestCase>
		where TTestCase : ITestCase
	{
		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		object?[] constructorArguments;
		IMessageBus messageBus;
		ITest test;
		Type testClass;
		MethodInfo testMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The test assembly unique ID.</param>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethodUniqueID">The test method unique ID.</param>
		/// <param name="testCaseUniqueID">The test case unique ID.</param>
		/// <param name="test">The test that this invocation belongs to.</param>
		/// <param name="testIndex">The test index for this test in the test case.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testClass">The test class that the test method belongs to.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethod">The test method that will be invoked.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestRunner(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			string testCaseUniqueID,
			ITest test,
			int testIndex,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
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

			SkipReason = skipReason;
			TestAssemblyUniqueID = Guard.ArgumentNotNull(nameof(testAssemblyUniqueID), testAssemblyUniqueID);
			TestCaseUniqueID = Guard.ArgumentNotNull(nameof(testCaseUniqueID), testCaseUniqueID);
			TestCollectionUniqueID = Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);
			TestClassUniqueID = testClassUniqueID;
			TestMethodArguments = testMethodArguments;
			TestMethodUniqueID = testMethodUniqueID;
			TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, testIndex);

			Guard.ArgumentValid(nameof(test), $"test.TestCase must implement {typeof(TTestCase).FullName}", test.TestCase is TTestCase);
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
		/// Gets or sets the display name of the invoked test.
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
		/// Gets or sets the skip reason for the test, if set.
		/// </summary>
		protected string? SkipReason { get; set; }

		/// <summary>
		/// Gets or sets the test to be run.
		/// </summary>
		protected ITest Test
		{
			get => test;
			set => test = Guard.ArgumentNotNull(nameof(Test), value);
		}

		/// <summary>
		/// Gets the test assembly unique ID.
		/// </summary>
		protected string TestAssemblyUniqueID { get; }

		/// <summary>
		/// Gets the test case to be run.
		/// </summary>
		protected TTestCase TestCase => (TTestCase)Test.TestCase;

		/// <summary>
		/// Gets the test case unique ID.
		/// </summary>
		protected string TestCaseUniqueID { get; }

		/// <summary>
		/// Gets or sets the runtime type of the class that contains the test method.
		/// </summary>
		protected Type TestClass
		{
			get => testClass;
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <summary>
		/// Gets the test class unique ID.
		/// </summary>
		protected string? TestClassUniqueID { get; }

		/// <summary>
		/// Gets the test collection unique ID.
		/// </summary>
		protected string TestCollectionUniqueID { get; }

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
		/// Maybe be <c>null</c> to indicate there are no arguments.
		/// </summary>
		protected object?[]? TestMethodArguments { get; set; }

		/// <summary>
		/// Gets the test method unique ID.
		/// </summary>
		protected string? TestMethodUniqueID { get; }

		/// <summary>
		/// Gets the test unique ID.
		/// </summary>
		protected string TestUniqueID { get; }

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
		/// Runs the test.
		/// </summary>
		/// <returns>Returns summary information about the test that was run.</returns>
		public async Task<RunSummary> RunAsync()
		{
			var runSummary = new RunSummary { Total = 1 };
			var output = string.Empty;

			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCaseUniqueID = TestCase.UniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestDisplayName = Test.DisplayName,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = TestUniqueID
			};

			if (!MessageBus.QueueMessage(testStarting))
				CancellationTokenSource.Cancel();
			else
			{
				AfterTestStarting();

				if (!string.IsNullOrEmpty(SkipReason))
				{
					runSummary.Skipped++;

					if (!MessageBus.QueueMessage(new TestSkipped(Test, SkipReason)))
						CancellationTokenSource.Cancel();
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
					TestResultMessage testResult;

					if (exception == null)
						testResult = new TestPassed(Test, runSummary.Time, output);
					// We don't want a strongly typed contract here; any exception can be a dynamically
					// skipped exception so long as its message starts with the special token.
					else if (exception.Message.StartsWith(DynamicSkipToken.Value))
					{
						testResult = new TestSkipped(Test, exception.Message.Substring(DynamicSkipToken.Value.Length));
						runSummary.Skipped++;
					}
					else
					{
						testResult = new TestFailed(Test, runSummary.Time, output, exception);
						runSummary.Failed++;
					}

					if (!CancellationTokenSource.IsCancellationRequested)
						if (!MessageBus.QueueMessage(testResult))
							CancellationTokenSource.Cancel();
				}

				Aggregator.Clear();
				BeforeTestFinished();

				if (Aggregator.HasExceptions)
					if (!MessageBus.QueueMessage(new TestCleanupFailure(Test, Aggregator.ToException()!)))
						CancellationTokenSource.Cancel();

				var testFinished = new _TestFinished
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					ExecutionTime = runSummary.Time,
					Output = output,
					TestCaseUniqueID = TestCase.UniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = TestUniqueID
				};

				if (!MessageBus.QueueMessage(testFinished))
					CancellationTokenSource.Cancel();
			}

			return runSummary;
		}

		/// <summary>
		/// Override this method to invoke the test.
		/// </summary>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <returns>Returns a tuple which includes the execution time (in seconds) spent running the
		/// test method, and any output that was returned by the test.</returns>
		protected abstract Task<Tuple<decimal, string>?> InvokeTestAsync(ExceptionAggregator aggregator);
	}
}
