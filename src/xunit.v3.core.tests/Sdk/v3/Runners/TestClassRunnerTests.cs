﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestClassRunnerTests
{
	[Fact]
	public static async void Messages()
	{
		var summary = new RunSummary { Total = 4, Failed = 2, Skipped = 1, Time = 21.12m };
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
				Assert.Equal(4, finished.TestsRun);
				Assert.Equal(1, finished.TestsSkipped);
			}
		);
	}

	[Fact]
	public static async void FailureInQueueOfTestClassStarting_DoesNotQueueTestClassFinished_DoesNotRunTestMethods()
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

		await Assert.ThrowsAsync<InvalidOperationException>(() => runner.RunAsync());

		var starting = Assert.Single(messages);
		Assert.IsType<_TestClassStarting>(starting);
		Assert.Empty(runner.MethodsRun);
	}

	[Fact]
	public static async void RunTestMethodAsync_AggregatorIncludesPassedInExceptions()
	{
		var messageBus = new SpyMessageBus();
		var ex = new DivideByZeroException();
		var runner = TestableTestClassRunner.Create(messageBus, aggregatorSeedException: ex);

		await runner.RunAsync();

		Assert.Same(ex, runner.RunTestMethodAsync_AggregatorResult);
		Assert.Empty(messageBus.Messages.OfType<_TestClassCleanupFailure>());
	}

	[Fact]
	public static async void FailureInAfterTestClassStarting_GivesErroredAggregatorToTestMethodRunner_NoCleanupFailureMessage()
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
	public static async void FailureInBeforeTestClassFinished_ReportsCleanupFailure_DoesNotIncludeExceptionsFromAfterTestClassStarting()
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
	public static async void Cancellation_TestClassStarting_DoesNotCallExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassStarting));
		var runner = TestableTestClassRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.False(runner.AfterTestClassStarting_Called);
		Assert.False(runner.BeforeTestClassFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestClassFinished_CallsExtensibilityCallbacks()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassFinished));
		var runner = TestableTestClassRunner.Create(messageBus);

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
		Assert.True(runner.AfterTestClassStarting_Called);
		Assert.True(runner.BeforeTestClassFinished_Called);
	}

	[Fact]
	public static async void Cancellation_TestClassCleanupFailure_SetsCancellationToken()
	{
		var messageBus = new SpyMessageBus(msg => !(msg is _TestClassCleanupFailure));
		var runner = TestableTestClassRunner.Create(messageBus);
		runner.BeforeTestClassFinished_Callback = aggregator => aggregator.Add(new Exception());

		await runner.RunAsync();

		Assert.True(runner.TokenSource.IsCancellationRequested);
	}

	[Fact]
	public static async void TestsAreGroupedByMethod()
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
	public static async void SignalingCancellationStopsRunningMethods()
	{
		var passing = Mocks.TestCase<ClassUnderTest>("Passing");
		var other = Mocks.TestCase<ClassUnderTest>("Other");
		var runner = TestableTestClassRunner.Create(testCases: new[] { passing, other }, cancelInRunTestMethodAsync: true);

		await runner.RunAsync();

		var tuple = Assert.Single(runner.MethodsRun);
		Assert.Equal("Passing", tuple.Item1?.Name);
	}

	[Fact]
	public static async void TestContextInspection()
	{
		var runner = TestableTestClassRunner.Create();

		await runner.RunAsync();

		Assert.NotNull(runner.AfterTestClassStarting_Context);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestClassStarting_Context.TestAssemblyStatus);
		Assert.Equal(TestEngineStatus.Running, runner.AfterTestClassStarting_Context.TestCollectionStatus);
		Assert.Equal(TestEngineStatus.Initializing, runner.AfterTestClassStarting_Context.TestClassStatus);
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

	public class TestCaseOrderer
	{
		[Fact]
		public static async void TestsOrdererIsUsedToDetermineRunOrder()
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
		public static async void TestCaseOrdererWhichThrowsLogsMessageAndDoesNotReorderTests()
		{
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
			var diagnosticMessage = Assert.Single(runner.DiagnosticMessages.Cast<_DiagnosticMessage>());
			Assert.StartsWith("Test case orderer 'TestClassRunnerTests+TestCaseOrderer+ThrowingOrderer' threw 'System.DivideByZeroException' during ordering: Attempted to divide by zero.", diagnosticMessage.Message);
		}

		class ThrowingOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : _ITestCase
			{
				throw new DivideByZeroException();
			}
		}
	}

	[Fact]
	public static async void TestClassMustHaveParameterlessConstructor()
	{
		var test = Mocks.TestCase<ClassWithConstructor>("Passing");
		var runner = TestableTestClassRunner.Create(testCases: new[] { test });

		await runner.RunAsync();

		var tcex = Assert.IsType<TestClassException>(runner.RunTestMethodAsync_AggregatorResult);
		Assert.Equal("A test class must have a parameterless constructor.", tcex.Message);
	}

	[Fact]
	public static async void ConstructorWithMissingArguments()
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
	public static async void ConstructorWithMatchingArguments()
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
		public ClassWithConstructor(int x, string y, decimal z) { }

		[Fact]
		public void Passing() { }
	}

	class TestableTestClassRunner : TestClassRunner<_ITestCase>
	{
		readonly object[] availableArguments;
		readonly bool cancelInRunTestMethodAsync;
		readonly ConstructorInfo? constructor;
		readonly RunSummary result;

		public List<Tuple<_IReflectionMethodInfo?, IReadOnlyCollection<_ITestCase>, object?[]>> MethodsRun = new();
		public Action<ExceptionAggregator> AfterTestClassStarting_Callback = _ => { };
		public bool AfterTestClassStarting_Called;
		public TestContext? AfterTestClassStarting_Context;
		public Action<ExceptionAggregator> BeforeTestClassFinished_Callback = _ => { };
		public bool BeforeTestClassFinished_Called;
		public TestContext? BeforeTestClassFinished_Context;
		public List<_MessageSinkMessage> DiagnosticMessages;
		public Exception? RunTestMethodAsync_AggregatorResult;
		public TestContext? RunTestMethodAsync_Context;
		public readonly CancellationTokenSource TokenSource;

		TestableTestClassRunner(
			_ITestClass? testClass,
			_IReflectionTypeInfo? @class,
			IReadOnlyCollection<_ITestCase> testCases,
			List<_MessageSinkMessage> diagnosticMessages,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			ConstructorInfo? constructor,
			object[] availableArguments,
			RunSummary result,
			bool cancelInRunTestMethodAsync)
				: base(testClass, @class, testCases, SpyMessageSink.Create(messages: diagnosticMessages), messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
		{
			DiagnosticMessages = diagnosticMessages;
			TokenSource = cancellationTokenSource;

			this.constructor = constructor;
			this.availableArguments = availableArguments;
			this.result = result;
			this.cancelInRunTestMethodAsync = cancelInRunTestMethodAsync;
		}

		public new _ITestClass? TestClass => base.TestClass;

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
			if (testCases == null)
				testCases = new[] { Mocks.TestCase<ClassUnderTest>("Passing") };
			if (availableArguments == null)
				availableArguments = new object[0];

			var firstTest = testCases.First();

			var aggregator = new ExceptionAggregator();
			if (aggregatorSeedException != null)
				aggregator.Add(aggregatorSeedException);

			return new TestableTestClassRunner(
				firstTest.TestMethod?.TestClass,
				(_IReflectionTypeInfo?)firstTest.TestMethod?.TestClass.Class,
				testCases,
				new List<_MessageSinkMessage>(),
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

		protected override Task AfterTestClassStartingAsync()
		{
			AfterTestClassStarting_Called = true;
			AfterTestClassStarting_Context = TestContext.Current;
			AfterTestClassStarting_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task BeforeTestClassFinishedAsync()
		{
			BeforeTestClassFinished_Called = true;
			BeforeTestClassFinished_Context = TestContext.Current;
			BeforeTestClassFinished_Callback(Aggregator);
			return Task.CompletedTask;
		}

		protected override Task<RunSummary> RunTestMethodAsync(
			_ITestMethod? testMethod,
			_IReflectionMethodInfo? method,
			IReadOnlyCollection<_ITestCase> testCases,
			object?[] constructorArguments)
		{
			if (cancelInRunTestMethodAsync)
				CancellationTokenSource.Cancel();

			RunTestMethodAsync_AggregatorResult = Aggregator.ToException();
			RunTestMethodAsync_Context = TestContext.Current;
			MethodsRun.Add(Tuple.Create(method, testCases, constructorArguments));
			return Task.FromResult(result);
		}

		protected override ConstructorInfo? SelectTestClassConstructor(_IReflectionTypeInfo @class)
		{
			return constructor ?? base.SelectTestClassConstructor(@class);
		}

		protected override bool TryGetConstructorArgument(
			ConstructorInfo constructor,
			int index,
			ParameterInfo parameter,
			[MaybeNullWhen(false)] out object argumentValue)
		{
			argumentValue = availableArguments.FirstOrDefault(arg => parameter.ParameterType.IsAssignableFrom(arg.GetType()));
			if (argumentValue != null)
				return true;

			return base.TryGetConstructorArgument(constructor, index, parameter, out argumentValue);
		}
	}
}
