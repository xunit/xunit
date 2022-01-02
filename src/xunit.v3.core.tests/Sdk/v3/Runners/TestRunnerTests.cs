using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestRunnerTests
{
	[Fact]
	public static async void Messages()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m);

		var result = await runner.RunAsync();

		Assert.Equal(21.12m, result.Time);
		Assert.False(runner.TokenSource.IsCancellationRequested);
		Assert.Collection(
			messageBus.Messages,
			msg =>
			{
				var testStarting = Assert.IsAssignableFrom<_TestStarting>(msg);
				Assert.Equal("Display Name", testStarting.TestDisplayName);
			},
			msg => { },  // Pass/fail/skip, will be tested elsewhere
			msg =>
			{
				var testFinished = Assert.IsAssignableFrom<_TestFinished>(msg);
				Assert.Equal(21.12m, testFinished.ExecutionTime);
				Assert.Empty(testFinished.Output);
			}
		);
	}

	[Fact]
	public static async void Passing()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m);

		var result = await runner.RunAsync();

		// Direct run summary
		Assert.Equal(1, result.Total);
		Assert.Equal(0, result.Failed);
		Assert.Equal(0, result.Skipped);
		Assert.Equal(21.12m, result.Time);
		// Pass message
		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		Assert.Equal(21.12m, passed.ExecutionTime);
		Assert.Empty(passed.Output);
		// Statuses
		Assert.NotNull(runner.AfterTestStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestStarting_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestFinished_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestState);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionMessages);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionParentIndices);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionStackTraces);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionTypes);
		Assert.Equal(21.12m, runner.BeforeTestFinished_Context.TestState.ExecutionTime);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.FailureCause);
		Assert.Equal(TestResult.Passed, runner.BeforeTestFinished_Context.TestState.Result);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestOutputHelper);
		Assert.Equal(string.Empty, runner.BeforeTestFinished_Context.TestOutputHelper.Output);
	}

	[Fact]
	public static async void Failing()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", runTime: 21.12m, lambda: () => Assert.True(false));

		var result = await runner.RunAsync();

		// Direct run summary
		Assert.Equal(1, result.Total);
		Assert.Equal(1, result.Failed);
		Assert.Equal(0, result.Skipped);
		Assert.Equal(21.12m, result.Time);
		// Fail message
		var failed = messageBus.Messages.OfType<_TestFailed>().Single();
		var failedStarting = Assert.Single(messageBus.Messages.OfType<_TestStarting>().Where(s => s.TestUniqueID == failed.TestUniqueID));
		Assert.Equal("Display Name", failedStarting.TestDisplayName);
		Assert.Equal(21.12m, failed.ExecutionTime);
		Assert.Empty(failed.Output);
		Assert.Equal("Xunit.Sdk.TrueException", failed.ExceptionTypes.Single());
		// Statuses
		Assert.NotNull(runner.AfterTestStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestStarting_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestFinished_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestState);
		Assert.Equal(failed.Messages, runner.BeforeTestFinished_Context.TestState.ExceptionMessages);
		Assert.Equal(failed.ExceptionParentIndices, runner.BeforeTestFinished_Context.TestState.ExceptionParentIndices);
		Assert.Equal(failed.StackTraces, runner.BeforeTestFinished_Context.TestState.ExceptionStackTraces);
		Assert.Equal(failed.ExceptionTypes, runner.BeforeTestFinished_Context.TestState.ExceptionTypes);
		Assert.Equal(21.12m, runner.BeforeTestFinished_Context.TestState.ExecutionTime);
		Assert.Equal(FailureCause.Assertion, runner.BeforeTestFinished_Context.TestState.FailureCause);
		Assert.Equal(TestResult.Failed, runner.BeforeTestFinished_Context.TestState.Result);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestOutputHelper);
		Assert.Equal(string.Empty, runner.BeforeTestFinished_Context.TestOutputHelper.Output);
	}

	[Fact]
	public static async void Skipping()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus, displayName: "Display Name", skipReason: "Please don't run me", runTime: 21.12m, lambda: () => Assert.True(false));

		var result = await runner.RunAsync();

		// Direct run summary
		Assert.Equal(1, result.Total);
		Assert.Equal(0, result.Failed);
		Assert.Equal(1, result.Skipped);
		Assert.Equal(0m, result.Time);
		// Skip message
		var skipped = Assert.Single(messageBus.Messages.OfType<_TestSkipped>());
		var skippedStarting = Assert.Single(messageBus.Messages.OfType<_TestStarting>().Where(s => s.TestUniqueID == skipped.TestUniqueID));
		Assert.Equal("Display Name", skippedStarting.TestDisplayName);
		Assert.Equal(0m, skipped.ExecutionTime);
		Assert.Empty(skipped.Output);
		Assert.Equal("Please don't run me", skipped.Reason);
		// Statuses
		Assert.NotNull(runner.AfterTestStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestStarting_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestStarting_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestClassStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestMethodStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestFinished_Context.TestCaseStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestFinished_Context.TestStatus);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestState);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionMessages);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionParentIndices);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionStackTraces);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.ExceptionTypes);
		Assert.Equal(0m, runner.BeforeTestFinished_Context.TestState.ExecutionTime);
		Assert.Null(runner.BeforeTestFinished_Context.TestState.FailureCause);
		Assert.Equal(TestResult.Skipped, runner.BeforeTestFinished_Context.TestState.Result);
		Assert.NotNull(runner.BeforeTestFinished_Context.TestOutputHelper);
		Assert.Equal(string.Empty, runner.BeforeTestFinished_Context.TestOutputHelper.Output);
	}

	[Fact]
	public static async void Output()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus, output: "This is my text output");

		await runner.RunAsync();

		var passed = messageBus.Messages.OfType<_TestPassed>().Single();
		Assert.Equal("This is my text output", passed.Output);
	}

	[Fact]
	public static async void FailureInQueueOfTestStarting_DoesNotQueueTestFinished_DoesNotInvokeTest()
	{
		var messages = new List<_MessageSinkMessage>();
		var messageBus = Substitute.For<IMessageBus>();
		messageBus
			.QueueMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var msg = callInfo.Arg<_MessageSinkMessage>();
				messages.Add(msg);

				if (msg is _TestStarting)
					throw new InvalidOperationException();

				return true;
			});
		var runner = TestableTestRunner.Create(messageBus);

		var ex = await Record.ExceptionAsync(() => runner.RunAsync());

		Assert.IsType<InvalidOperationException>(ex);
		var starting = Assert.Single(messages);
		Assert.IsAssignableFrom<_TestStarting>(starting);
		Assert.False(runner.InvokeTestAsync_Called);
	}

	[Fact]
	public static async void WithPreSeededException_ReturnsTestFailed_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionTypes.Single());
		Assert.Empty(messageBus.Messages.OfType<_TestCleanupFailure>());
	}

	[Fact]
	public static async void FailureInAfterTestStarting_ReturnsTestFailed_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestRunner.Create(messageBus);
		var ex = new DivideByZeroException();
		runner.AfterTestStarting_Callback = aggregator => aggregator.Add(ex);

		await runner.RunAsync();

		var failed = Assert.Single(messageBus.Messages.OfType<_TestFailed>());
		Assert.Equal(typeof(DivideByZeroException).FullName, failed.ExceptionTypes.Single());
		Assert.Empty(messageBus.Messages.OfType<_TestCleanupFailure>());
	}

	[Fact]
	public static async void FailureInBeforeTestFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestStarting()
	{
		var messageBus = new SpyMessageBus();
		var testCase = Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages");
		var runner = TestableTestRunner.Create(messageBus, testCase);
		var startingException = new DivideByZeroException();
		var finishedException = new InvalidOperationException();
		runner.AfterTestStarting_Callback = aggregator => aggregator.Add(startingException);
		runner.BeforeTestFinished_Callback = aggregator => aggregator.Add(finishedException);

		await runner.RunAsync();

		var cleanupFailure = Assert.Single(messageBus.Messages.OfType<_TestCleanupFailure>());
		Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
	}

	[Fact]
	public static async void Cancellation_TestStarting_DoesNotCallExtensibilityMethods()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestStarting));
		var runner = TestableTestRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestStarting_Called);
		Assert.False(runner.BeforeTestFinished_Called);
	}

	[Theory(DisableDiscoveryEnumeration = true)]
	[InlineData(typeof(_TestPassed), true, null)]
	[InlineData(typeof(_TestFailed), false, null)]
	[InlineData(typeof(_TestSkipped), false, "Please skip me")]
	[InlineData(typeof(_TestFinished), true, null)]
	public static async void Cancellation_AllOthers_CallsExtensibilityMethods(
		Type messageTypeToCancelOn,
		bool shouldTestPass,
		string? skipReason = null)
	{
		var messageBus = new SpyMessageBus(msg => !(messageTypeToCancelOn.IsAssignableFrom(msg.GetType())));
		var runner = TestableTestRunner.Create(messageBus, skipReason: skipReason, lambda: () => Assert.True(shouldTestPass));

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestStarting_Called);
		Assert.True(runner.BeforeTestFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestCleanupFailure));
		var runner = TestableTestRunner.Create(messageBus);
		runner.BeforeTestFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	class TestableTestRunner : TestRunner<TestRunnerContext>
	{
		readonly ExceptionAggregator aggregator;
		readonly object?[] constructorArguments;
		readonly Action? lambda;
		readonly IMessageBus messageBus;
		readonly string output;
		readonly decimal runTime;
		readonly string? skipReason;
		readonly _ITest test;
		readonly Type testClass;
		readonly MethodInfo testMethod;
		readonly object?[]? testMethodArguments;

		public bool InvokeTestAsync_Called;
		public Action<ExceptionAggregator> AfterTestStarting_Callback = _ => { };
		public TestContext? AfterTestStarting_Context;
		public bool AfterTestStarting_Called;
		public Action<ExceptionAggregator> BeforeTestFinished_Callback = _ => { };
		public bool BeforeTestFinished_Called;
		public TestContext? BeforeTestFinished_Context;
		public CancellationTokenSource TokenSource;

		TestableTestRunner(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			decimal runTime,
			string output,
			Action? lambda)
		{
			this.test = test;
			this.messageBus = messageBus;
			this.testClass = testClass;
			this.constructorArguments = constructorArguments;
			this.testMethod = testMethod;
			this.testMethodArguments = testMethodArguments;
			this.skipReason = skipReason;
			this.aggregator = aggregator;
			TokenSource = cancellationTokenSource;

			this.runTime = runTime;
			this.output = output;
			this.lambda = lambda;
		}

		public static TestableTestRunner Create(
			IMessageBus messageBus,
			_ITestCase? testCase = null,
			string displayName = "MockDisplayName",
			string? skipReason = null,
			decimal runTime = 0m,
			string output = "",
			Exception? aggregatorSeedException = null,
			Action? lambda = null)
		{
			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException != null)
				aggregator.Add(aggregatorSeedException);
			if (testCase == null)
				testCase = Mocks.TestCase<object>("ToString");
			var test = Mocks.Test(testCase, displayName, "test-id");

			return new TestableTestRunner(
				test,
				messageBus,
				typeof(object),
				new object[0],
				typeof(object).GetMethod("ToString")!,
				new object[0],
				skipReason,
				aggregator,
				new CancellationTokenSource(),
				runTime,
				output,
				lambda
			);
		}

		protected override void AfterTestStarting(TestRunnerContext ctxt)
		{
			AfterTestStarting_Called = true;
			AfterTestStarting_Context = TestContext.Current;
			AfterTestStarting_Callback(aggregator);
		}

		protected override void BeforeTestFinished(TestRunnerContext ctxt)
		{
			BeforeTestFinished_Called = true;
			BeforeTestFinished_Context = TestContext.Current;
			BeforeTestFinished_Callback(aggregator);
		}

		protected override ValueTask<(decimal ExecutionTime, string Output)?> InvokeTestAsync(TestRunnerContext ctxt)
		{
			if (lambda != null)
				aggregator.Run(lambda);

			InvokeTestAsync_Called = true;

			return new((runTime, output));
		}

		public ValueTask<RunSummary> RunAsync() =>
			RunAsync(new(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, aggregator, TokenSource));
	}
}
