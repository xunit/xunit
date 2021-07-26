using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base class that provides default behavior when running tests in a test method.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestMethodRunner<TTestCase>
		where TTestCase : _ITestCase
	{
		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		IMessageBus messageBus;
		IReadOnlyCollection<TTestCase> testCases;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testMethod">The test method under test. May be <c>null</c> for test cases that do not support methods.</param>
		/// <param name="class">The CLR class that contains the test method. May be <c>null</c> for test cases that do not
		/// support classes and methods.</param>
		/// <param name="method">The CLR method that contains the tests to be run. May be <c>null</c> for test cases that do not
		/// support methods.</param>
		/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestMethodRunner(
			_ITestMethod? testMethod,
			_IReflectionTypeInfo? @class,
			_IReflectionMethodInfo? method,
			IReadOnlyCollection<TTestCase> testCases,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			TestMethod = testMethod;
			Class = @class;
			Method = method;
			this.testCases = Guard.ArgumentNotNullOrEmpty(nameof(testCases), testCases);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);
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
		/// Gets or sets the CLR class that contains the test method.
		/// </summary>
		protected _IReflectionTypeInfo? Class { get; set; }

		/// <summary>
		/// Gets or sets the message bus to report run status to.
		/// </summary>
		protected IMessageBus MessageBus
		{
			get => messageBus;
			set => messageBus = Guard.ArgumentNotNull(nameof(MessageBus), value);
		}

		/// <summary>
		/// Gets or sets the CLR method that contains the tests to be run.
		/// </summary>
		protected _IReflectionMethodInfo? Method { get; set; }

		/// <summary>
		/// Gets or sets the test cases to be run.
		/// </summary>
		protected IReadOnlyCollection<TTestCase> TestCases
		{
			get => testCases;
			set => testCases = Guard.ArgumentNotNull(nameof(TestCases), value);
		}

		/// <summary>
		/// Gets or sets the test method that contains the test cases.
		/// </summary>
		protected _ITestMethod? TestMethod { get; set; }

		/// <summary>
		/// This method is called just after <see cref="_TestMethodStarting"/> is sent, but before any test cases are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual void AfterTestMethodStarting()
		{ }

		/// <summary>
		/// This method is called just before <see cref="_TestMethodFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual void BeforeTestMethodFinished()
		{ }

		/// <summary>
		/// Runs the tests in the test method.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		public async Task<RunSummary> RunAsync()
		{
			SetTestContext(TestEngineStatus.Initializing);

			var methodSummary = new RunSummary();
			var testCollection = TestCases.First().TestCollection;
			var testAssemblyUniqueID = testCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = testCollection.UniqueID;
			var testClassUniqueID = TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = TestMethod?.UniqueID;

			if (TestMethod != null)
			{
				var methodStarting = new _TestMethodStarting
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethod = TestMethod.Method.Name,
					TestMethodUniqueID = testMethodUniqueID
				};

				if (!MessageBus.QueueMessage(methodStarting))
				{
					CancellationTokenSource.Cancel();
					return methodSummary;
				}
			}

			try
			{
				AfterTestMethodStarting();

				SetTestContext(TestEngineStatus.Running);

				methodSummary = await RunTestCasesAsync();

				SetTestContext(TestEngineStatus.CleaningUp);

				Aggregator.Clear();
				BeforeTestMethodFinished();

				if (Aggregator.HasExceptions)
				{
					var methodCleanupFailure = _TestMethodCleanupFailure.FromException(Aggregator.ToException()!, testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID, testMethodUniqueID);
					if (!MessageBus.QueueMessage(methodCleanupFailure))
						CancellationTokenSource.Cancel();
				}

				return methodSummary;
			}
			finally
			{
				if (TestMethod != null)
				{
					var testMethodFinished = new _TestMethodFinished
					{
						AssemblyUniqueID = testAssemblyUniqueID,
						ExecutionTime = methodSummary.Time,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestsFailed = methodSummary.Failed,
						TestsRun = methodSummary.Total,
						TestsSkipped = methodSummary.Skipped
					};

					if (!MessageBus.QueueMessage(testMethodFinished))
						CancellationTokenSource.Cancel();
				}
			}
		}

		/// <summary>
		/// Runs the list of test cases. By default, it runs the cases in order, synchronously.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected virtual async Task<RunSummary> RunTestCasesAsync()
		{
			var summary = new RunSummary();

			foreach (var testCase in TestCases)
			{
				summary.Aggregate(await RunTestCaseAsync(testCase));
				if (CancellationTokenSource.IsCancellationRequested)
					break;
			}

			return summary;
		}

		/// <summary>
		/// Override this method to run an individual test case.
		/// </summary>
		/// <param name="testCase">The test case to be run.</param>
		/// <returns>Returns summary information about the test case run.</returns>
		protected abstract Task<RunSummary> RunTestCaseAsync(TTestCase testCase);

		/// <summary>
		/// Sets the current <see cref="TestContext"/> for the current test method and the given test method status.
		/// Does nothing when <see cref="TestMethod"/> is <c>null</c>.
		/// </summary>
		/// <param name="testMethodStatus">The current test method status.</param>
		protected virtual void SetTestContext(TestEngineStatus testMethodStatus)
		{
			if (TestMethod != null)
				TestContext.SetForTestMethod(TestMethod, testMethodStatus);
		}
	}
}
