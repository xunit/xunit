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

public static class TestClassRunnerTests
{
	[Fact]
	public static async ValueTask Messages()
	{
		var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
		var messageBus = new SpyMessageBus();
		var testCase = Mocks.TestCase<ClassUnderTest>("Passing");
		var runner = TestableTestClassRunner.Create(messageBus, new[] { testCase }, result: summary);

		var result = await runner.RunAsync();

		Assert.Equal(result.Total, summary.Total);
		Assert.Equal(result.Failed, summary.Failed);
		Assert.Equal(result.Skipped, summary.Skipped);
		Assert.Equal(result.Time, summary.Time);
		Assert.False(runner.TokenSource.IsCancellationRequested);
		Assert.Collection(
			messageBus.Messages,
			msg =>
			{
				var starting = Assert.IsType<_TestClassStarting>(msg);
				Assert.Equal("assembly-id", starting.AssemblyUniqueID);
				Assert.Equal("TestClassRunnerTests+ClassUnderTest", starting.TestClass);
				Assert.Equal("class-id", starting.TestClassUniqueID);
				Assert.Equal("collection-id", starting.TestCollectionUniqueID);
			},
			msg =>
			{
				var finished = Assert.IsType<_TestClassFinished>(msg);
				Assert.Equal("assembly-id", finished.AssemblyUniqueID);
				Assert.Equal(21.12m, finished.ExecutionTime);
				Assert.Equal("class-id", finished.TestClassUniqueID);
				Assert.Equal("collection-id", finished.TestCollectionUniqueID);
				Assert.Equal(2, finished.TestsFailed);
				Assert.Equal(3, finished.TestsNotRun);
				Assert.Equal(1, finished.TestsSkipped);
				Assert.Equal(9, finished.TestsTotal);
			}
		);
	}

	[Fact]
	public static async ValueTask FailureInQueueOfTestClassStarting_DoesNotQueueTestClassFinished_DoesNotRunTestMethods()
	{
		var messages = new List<_MessageSinkMessage>();
		var messageBus = Substitute.For<IMessageBus>();
		messageBus
			.QueueMessage(null!)
			.ReturnsForAnyArgs(callInfo =>
			{
				var msg = callInfo.Arg<_MessageSinkMessage>();
				messages.Add(msg);

				if (msg is _TestClassStarting)
					throw new InvalidOperationException();

				return true;
			});
		var runner = TestableTestClassRunner.Create(messageBus);

		var ex = await Record.ExceptionAsync(() => runner.RunAsync());

		Assert.IsType<InvalidOperationException>(ex);
		var starting = Assert.Single(messages);
		Assert.IsType<_TestClassStarting>(starting);
		Assert.Empty(runner.MethodsRun);
	}

	[Fact]
	public static async ValueTask RunTestMethodAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestClassRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestMethodAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestClassCleanupFailure>());
	}

	[Fact]
	public static async ValueTask FailureInAfterTestClassStarting_GivesErroredAggregatorToTestMethodRunner_NoCleanupFailureMessage()
	{
		var messageBus = new SpyMessageBus();
		var runner = TestableTestClassRunner.Create(messageBus);
		var ex = new DivideByZeroException();
		runner.AfterTestClassStarting_Callback = aggregator => aggregator.Add(ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestMethodAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestClassCleanupFailure>());
	}

	[Fact]
	public static async ValueTask FailureInBeforeTestClassFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestClassStarting()
	{
		var messageBus = new SpyMessageBus();
		var testCases = new[] { Mocks.TestCase<TestAssemblyRunnerTests.RunAsync>("Messages") };
		var runner = TestableTestClassRunner.Create(messageBus, testCases);
		var startingException = new DivideByZeroException();
		var finishedException = new InvalidOperationException();
		runner.AfterTestClassStarting_Callback = aggregator => aggregator.Add(startingException);
		runner.BeforeTestClassFinished_Callback = aggregator => aggregator.Add(finishedException);

		await runner.RunAsync();

		var cleanupFailure = Assert.Single(messageBus.Messages.OfType<_TestClassCleanupFailure>());
		Assert.Equal(typeof(InvalidOperationException).FullName, cleanupFailure.ExceptionTypes.Single());
	}

	[Fact]
	public static async ValueTask Cancellation_TestClassStarting_DoesNotCallExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassStarting));
		var runner = TestableTestClassRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestClassStarting_Called);
		Assert.False(runner.BeforeTestClassFinished_Called);
	}

	[Fact]
	public static async ValueTask Cancellation_TestClassFinished_CallsExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassFinished));
		var runner = TestableTestClassRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestClassStarting_Called);
		Assert.True(runner.BeforeTestClassFinished_Called);
	}

	[Fact]
	public static async ValueTask Cancellation_TestClassCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassCleanupFailure));
		var runner = TestableTestClassRunner.Create(messageBus);
		runner.BeforeTestClassFinished_Callback = aggregator => aggregator.Add(new DivideByZeroException());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Fact]
	public static async ValueTask TestsAreGroupedByMethod()
	{
		var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
		var passing2 = Mocks.TestCase<ClassUnderTest>("Passing");
		var other1 = Mocks.TestCase<ClassUnderTest>("Other");
		var other2 = Mocks.TestCase<ClassUnderTest>("Other");
		var runner = TestableTestClassRunner.Create(testCases: new[] { passing1, other1, other2, passing2 });

		await runner.RunAsync();

		Assert.Collection(
			runner.MethodsRun,
			tuple =>
			{
				Assert.Equal("Passing", tuple.Item1?.Name);
				Assert.Collection(tuple.Item2,
					testCase => Assert.Same(passing1, testCase),
					testCase => Assert.Same(passing2, testCase)
				);
			},
			tuple =>
			{
				Assert.Equal("Other", tuple.Item1?.Name);
				Assert.Collection(tuple.Item2,
					testCase => Assert.Same(other1, testCase),
					testCase => Assert.Same(other2, testCase)
				);
			}
		);
	}

	[Fact]
	public static async ValueTask SignalingCancellationStopsRunningMethods()
	{
		var passing = Mocks.TestCase<ClassUnderTest>("Passing");
		var other = Mocks.TestCase<ClassUnderTest>("Other");
		var runner = TestableTestClassRunner.Create(testCases: new[] { passing, other }, cancelInRunTestMethodAsync: true);

		await runner.RunAsync();

		var tuple = Assert.Single(runner.MethodsRun);
		Assert.Equal("Passing", tuple.Item1?.Name);
	}

	[Fact]
	public static async ValueTask TestContextInspection()
	{
		var runner = TestableTestClassRunner.Create();

		await runner.RunAsync();

		Assert.NotNull(runner.AfterTestClassStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestClassStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestClassStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestClassStarting_Context.TestClassStatus);
		Assert.Equal(TestPipelineStage.TestClassExecution, runner.AfterTestClassStarting_Context.PipelineStage);
		Assert.Null(runner.AfterTestClassStarting_Context.TestMethodStatus);
		Assert.Null(runner.AfterTestClassStarting_Context.TestCaseStatus);
		Assert.Null(runner.AfterTestClassStarting_Context.TestStatus);
		Assert.Same(runner.TestClass, runner.AfterTestClassStarting_Context.TestClass);

		Assert.NotNull(runner.RunTestMethodAsync_Context);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestMethodAsync_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestMethodAsync_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Running, runner.RunTestMethodAsync_Context.TestClassStatus);
		Assert.Null(runner.RunTestMethodAsync_Context.TestMethodStatus);
		Assert.Null(runner.RunTestMethodAsync_Context.TestCaseStatus);
		Assert.Null(runner.RunTestMethodAsync_Context.TestStatus);
		Assert.Same(runner.TestClass, runner.RunTestMethodAsync_Context.TestClass);

		Assert.NotNull(runner.BeforeTestClassFinished_Context);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestClassFinished_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.BeforeTestClassFinished_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.CleaningUp, runner.BeforeTestClassFinished_Context.TestClassStatus);
		Assert.Null(runner.BeforeTestClassFinished_Context.TestMethodStatus);
		Assert.Null(runner.BeforeTestClassFinished_Context.TestCaseStatus);
		Assert.Null(runner.BeforeTestClassFinished_Context.TestStatus);
		Assert.Same(runner.TestClass, runner.BeforeTestClassFinished_Context.TestClass);
	}

	public static class TestCaseOrderer
	{
		[Fact]
		public static async ValueTask TestsOrdererIsUsedToDetermineRunOrder()
		{
			var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
			var passing2 = Mocks.TestCase<ClassUnderTest>("Passing");
			var other1 = Mocks.TestCase<ClassUnderTest>("Other");
			var other2 = Mocks.TestCase<ClassUnderTest>("Other");
			var runner = TestableTestClassRunner.Create(testCases: new[] { passing1, other1, passing2, other2 }, orderer: new MockTestCaseOrderer(reverse: true));

			await runner.RunAsync();

			Assert.Collection(
				runner.MethodsRun,
				tuple =>
				{
					Assert.Equal("Other", tuple.Item1?.Name);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(other2, testCase),
						testCase => Assert.Same(other1, testCase)
					);
				},
				tuple =>
				{
					Assert.Equal("Passing", tuple.Item1?.Name);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(passing2, testCase),
						testCase => Assert.Same(passing1, testCase)
					);
				}
			);
		}

		[Fact]
		public static async ValueTask TestCaseOrdererWhichThrowsLogsMessageAndDoesNotReorderTests()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var passing1 = Mocks.TestCase<ClassUnderTest>("Passing");
			var passing2 = Mocks.TestCase<ClassUnderTest>("Passing");
			var other1 = Mocks.TestCase<ClassUnderTest>("Other");
			var other2 = Mocks.TestCase<ClassUnderTest>("Other");
			var runner = TestableTestClassRunner.Create(testCases: new[] { passing1, other1, passing2, other2 }, orderer: new ThrowingOrderer());

			await runner.RunAsync();

			Assert.Collection(
				runner.MethodsRun,
				tuple =>
				{
					Assert.Equal("Passing", tuple.Item1?.Name);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(passing1, testCase),
						testCase => Assert.Same(passing2, testCase)
					);
				},
				tuple =>
				{
					Assert.Equal("Other", tuple.Item1?.Name);
					Assert.Collection(tuple.Item2,
						testCase => Assert.Same(other1, testCase),
						testCase => Assert.Same(other2, testCase)
					);
				}
			);
			var diagnosticMessage = Assert.Single(spy.Messages.Cast<_DiagnosticMessage>());
			Assert.StartsWith("Test case orderer 'TestClassRunnerTests+TestCaseOrderer+ThrowingOrderer' threw 'System.DivideByZeroException' during ordering: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class ThrowingOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, _ITestCase
			{
				throw new DivideByZeroException();
			}
		}
	}

	[Fact]
	public static async ValueTask TestClassMustHaveParameterlessConstructor()
	{
		var test = Mocks.TestCase<ClassWithConstructor>("Passing");
		var runner = TestableTestClassRunner.Create(testCases: new[] { test });

		await runner.RunAsync();

		var tcex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
		Assert.Equal("A test class must have a parameterless constructor.", tcex.Message);
	}

	[Fact]
	public static async ValueTask ConstructorWithMissingArguments()
	{
		var test = Mocks.TestCase<ClassWithConstructor>("Passing");
		var constructor = typeof(ClassWithConstructor).GetConstructors().Single();
		var args = new object[] { "Hello, world!" };
		var runner = TestableTestClassRunner.Create(testCases: new[] { test }, constructor: constructor, availableArguments: args);

		await runner.RunAsync();

		var tcex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
		Assert.Equal("The following constructor parameters did not have matching arguments: Int32 x, Decimal z", tcex.Message);
	}

	[Fact]
	public static async ValueTask ConstructorWithMatchingArguments()
	{
		var test = Mocks.TestCase<ClassWithConstructor>("Passing");
		var constructor = typeof(ClassWithConstructor).GetConstructors().Single();
		var args = new object[] { "Hello, world!", 21.12m, 42, DateTime.Now };
		var runner = TestableTestClassRunner.Create(testCases: new[] { test }, constructor: constructor, availableArguments: args);

		await runner.RunAsync();

		var tuple = Assert.Single(runner.MethodsRun);
		Assert.Collection(
			tuple.Item3,
			arg => Assert.Equal(42, arg),
			arg => Assert.Equal("Hello, world!", arg),
			arg => Assert.Equal(21.12m, arg)
		);
		Assert.Null(runner.RunTestMethodAsync_AggregatorResult);
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { }

		[Fact]
		public void Other() { }
	}

	class ClassWithConstructor
	{
#pragma warning disable xUnit1041 // Fixture arguments to test classes must have fixture sources
		public ClassWithConstructor(int x, string y, decimal z) { }
#pragma warning restore xUnit1041

		[Fact]
		public void Passing() { }
	}

	class TestableTestClassRunner : TestClassRunner<TestClassRunnerContext<_ITestCase>, _ITestCase>
	{
		readonly ExceptionAggregator aggregator;
		readonly object[] availableArguments;
		readonly bool cancelInRunTestMethodAsync;
		readonly _IReflectionTypeInfo @class;
		readonly ConstructorInfo? constructor;
		readonly IMessageBus messageBus;
		readonly RunSummary result;
		readonly ITestCaseOrderer testCaseOrderer;
		readonly IReadOnlyCollection<_ITestCase> testCases;

		public Action<ExceptionAggregator> AfterTestClassStarting_Callback = _ => { };
		public bool AfterTestClassStarting_Called;
		public TestContext? AfterTestClassStarting_Context;
		public Action<ExceptionAggregator> BeforeTestClassFinished_Callback = _ => { };
		public bool BeforeTestClassFinished_Called;
		public TestContext? BeforeTestClassFinished_Context;
		public List<Tuple<_IReflectionMethodInfo?, IReadOnlyCollection<_ITestCase>, object?[]>> MethodsRun = new();
		public Exception? RunTestMethodAsync_AggregatorResult;
		public TestContext? RunTestMethodAsync_Context;
		public readonly _ITestClass TestClass;
		public readonly CancellationTokenSource TokenSource;

		TestableTestClassRunner(
			_ITestClass testClass,
			_IReflectionTypeInfo @class,
			IReadOnlyCollection<_ITestCase> testCases,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			ConstructorInfo? constructor,
			object[] availableArguments,
			RunSummary result,
			bool cancelInRunTestMethodAsync)
		{
			TestClass = testClass;
			this.@class = @class;
			this.testCases = testCases;
			this.messageBus = messageBus;
			this.testCaseOrderer = testCaseOrderer;
			this.aggregator = aggregator;
			TokenSource = cancellationTokenSource;
			this.constructor = constructor;
			this.availableArguments = availableArguments;
			this.result = result;
			this.cancelInRunTestMethodAsync = cancelInRunTestMethodAsync;
		}

		public static TestableTestClassRunner Create(
			IMessageBus? messageBus = null,
			_ITestCase[]? testCases = null,
			ITestCaseOrderer? orderer = null,
			ConstructorInfo? constructor = null,
			object[]? availableArguments = null,
			RunSummary? result = null,
			Exception? aggregatorSeedException = null,
			bool cancelInRunTestMethodAsync = false)
		{
			if (testCases is null)
				testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };
			if (availableArguments is null)
				availableArguments = Array.Empty<object>();

			var firstTest = testCases.First();

			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException is not null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestClassRunner(
				firstTest.TestClass ?? throw new InvalidOperationException("testCase.TestClass must not be null"),
				firstTest.TestClass.Class as _IReflectionTypeInfo ?? throw new InvalidOperationException("testCase.TestClass.Class must be based on reflection"),
				testCases,
				messageBus ?? new SpyMessageBus(),
				orderer ?? new MockTestCaseOrderer(),
				aggregator,
				new CancellationTokenSource(),
				constructor,
				availableArguments,
				result ?? new RunSummary(),
				cancelInRunTestMethodAsync
			);
		}

		protected override ValueTask AfterTestClassStartingAsync(TestClassRunnerContext<_ITestCase> ctxt)
		{
			AfterTestClassStarting_Called = true;
			AfterTestClassStarting_Context = TestContext.Current;
			AfterTestClassStarting_Callback(ctxt.Aggregator);
			return default;
		}

		protected override ValueTask BeforeTestClassFinishedAsync(TestClassRunnerContext<_ITestCase> ctxt)
		{
			BeforeTestClassFinished_Called = true;
			BeforeTestClassFinished_Context = TestContext.Current;
			BeforeTestClassFinished_Callback(ctxt.Aggregator);
			return default;
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestClassRunnerContext<_ITestCase>(TestClass, @class, testCases, ExplicitOption.Off, messageBus, testCaseOrderer, aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		protected override ValueTask<RunSummary> RunTestMethodAsync(
			TestClassRunnerContext<_ITestCase> ctxt,
			_ITestMethod? testMethod,
			_IReflectionMethodInfo? method,
			IReadOnlyCollection<_ITestCase> testCases,
			object?[] constructorArguments)
		{
			if (cancelInRunTestMethodAsync)
				ctxt.CancellationTokenSource.Cancel();

			RunTestMethodAsync_AggregatorResult = ctxt.Aggregator.ToException();
			RunTestMethodAsync_Context = TestContext.Current;
			MethodsRun.Add(Tuple.Create(method, testCases, constructorArguments));
			return new(result);
		}

		protected override ConstructorInfo? SelectTestClassConstructor(TestClassRunnerContext<_ITestCase> ctxt)
		{
			return constructor ?? base.SelectTestClassConstructor(ctxt);
		}

		protected override bool TryGetConstructorArgument(
			TestClassRunnerContext<_ITestCase> ctxt,
			ConstructorInfo constructor,
			int index,
			ParameterInfo parameter,
			out object argumentValue)
		{
			var resultValue = availableArguments.FirstOrDefault(arg => parameter.ParameterType.IsAssignableFrom(arg.GetType()));
			if (resultValue is null)
			{
				var result = base.TryGetConstructorArgument(ctxt, constructor, index, parameter, out resultValue);
				if (result == false || resultValue is null)
				{
					argumentValue = null!;
					return false;
				}
			}

			argumentValue = resultValue;
			return true;
		}
	}
}
