using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public static class TestClassRunnerTests
{
	public class Messages
	{
		[Fact]
		public async ValueTask OnTestClassCleanupFailure()
		{
			var runner = new TestableTestClassRunner();
			var ex = Record.Exception(ThrowException);

			await runner.OnTestClassCleanupFailure(ex!);

			var message = Assert.Single(runner.MessageBus.Messages);
			var failure = Assert.IsAssignableFrom<ITestClassCleanupFailure>(message);

			VerifyTestClassMessage(failure);
			Assert.Equal(-1, failure.ExceptionParentIndices.Single());
			Assert.Equal(typeof(DivideByZeroException).FullName, failure.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", failure.Messages.Single());
			Assert.NotEmpty(failure.StackTraces.Single()!);
		}

		[Fact]
		public async ValueTask OnTestClassFinished()
		{
			var runner = new TestableTestClassRunner();
			var summary = new RunSummary { Total = 2112, Failed = 42, Skipped = 21, NotRun = 9, Time = 123.45m };

			await runner.OnTestClassFinished(summary);

			var message = Assert.Single(runner.MessageBus.Messages);
			var finished = Assert.IsAssignableFrom<ITestClassFinished>(message);

			VerifyTestClassMessage(finished);
			Assert.Equal(123.45m, finished.ExecutionTime);
			Assert.Equal(42, finished.TestsFailed);
			Assert.Equal(9, finished.TestsNotRun);
			Assert.Equal(21, finished.TestsSkipped);
			Assert.Equal(2112, finished.TestsTotal);
		}

		[Fact]
		public async ValueTask OnTestClassStarting()
		{
			var runner = new TestableTestClassRunner();

			await runner.OnTestClassStarting();

			var message = Assert.Single(runner.MessageBus.Messages);
			var starting = Assert.IsAssignableFrom<ITestClassStarting>(message);

			VerifyTestClassMessage(starting);
			Assert.Equal("test-class-name", starting.TestClassName);
			Assert.Equal("test-class-namespace", starting.TestClassNamespace);
			Assert.Equal("test-class-simple-name", starting.TestClassSimpleName);
			Assert.Equivalent(TestData.DefaultTraits, starting.Traits);
		}

		static void ThrowException() =>
			throw new DivideByZeroException();

		static void VerifyTestClassMessage(ITestClassMessage message)
		{
			Assert.Equal("assembly-id", message.AssemblyUniqueID);
			Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
			Assert.Equal("test-class-id", message.TestClassUniqueID);
		}
	}

	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestClassStarting()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestClassRunner
			{
				OnTestClassStarting__Result = false,
				RunTestMethodAsync__Result = summary,
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				// RunTestMethodAsync
				"OnTestClassFinished(summary: { Total = 0 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestClassFinished()
		{
			var runner = new TestableTestClassRunner { OnTestClassFinished__Result = false };

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"RunTestMethodAsync(testMethod: \"test-method\", constructorArguments: [])",
				"OnTestClassFinished(summary: { Total = 0 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestClassCleanupFailure()
		{
			// Need to throw in OnTestClassFinished to get OnTestClassCleanupFailure to trigger
			var runner = new TestableTestClassRunner
			{
				OnTestClassCleanupFailure__Result = false,
				OnTestClassFinished__Lambda = () => throw new DivideByZeroException(),
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"RunTestMethodAsync(testMethod: \"test-method\", constructorArguments: [])",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}
	}

	public class ExceptionHandling
	{
		[Fact]
		public static async ValueTask NoExceptions()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestClassRunner { RunTestMethodAsync__Result = summary };

			var result = await runner.RunAsync();

			Assert.Equal(summary, result);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"RunTestMethodAsync(testMethod: \"test-method\", constructorArguments: [])",
				"OnTestClassFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestClassStarting()
		{
			var runner = new TestableTestClassRunner { OnTestClassStarting__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"FailTestMethod(testMethod: \"test-method\", constructorArguments: [], exception: typeof(DivideByZeroException))",
				"OnTestClassFinished(summary: { Total = 0 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestClassFinished()
		{
			var runner = new TestableTestClassRunner { OnTestClassFinished__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"RunTestMethodAsync(testMethod: \"test-method\", constructorArguments: [])",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestClassCleanupFailure()
		{
			// Need to throw in OnTestClassFinished to get OnTestClassCleanupFailure to trigger
			var runner = new TestableTestClassRunner
			{
				OnTestClassCleanupFailure__Lambda = () => throw new DivideByZeroException(),
				OnTestClassFinished__Lambda = () => throw new ArgumentException(),
			};

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestClassStarting",
				"RunTestMethodAsync(testMethod: \"test-method\", constructorArguments: [])",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			Assert.Collection(
				runner.MessageBus.Messages,
				message => Assert.IsAssignableFrom<ITestClassStarting>(message),
				message =>
				{
					var errorMessage = Assert.IsAssignableFrom<IErrorMessage>(message);
					Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
					Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
					Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
					Assert.NotEmpty(errorMessage.StackTraces.Single()!);
				}
			);
		}
	}

	class TestableTestClassRunner(ITestCase? testCase = null) :
		TestClassRunner<TestClassRunnerContext<ITestClass, ITestCase>, ITestClass, ITestMethod, ITestCase>
	{
		readonly ITestCase testCase = testCase ?? Mocks.TestCase();

		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		ITestClass TestClass => Guard.ArgumentNotNull(testCase.TestClass);

		public RunSummary FailTestMethod__Result = new();

		protected override ValueTask<RunSummary> FailTestMethod(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			ITestMethod? testMethod,
			IReadOnlyCollection<ITestCase> testCases,
			object?[] constructorArguments,
			Exception exception)
		{
			Invocations.Add($"FailTestMethod(testMethod: {ArgumentFormatter.Format(testMethod?.MethodName)}, constructorArguments: {ArgumentFormatter.Format(constructorArguments)}, exception: {TypeName(exception)})");

			return new(FailTestMethod__Result);
		}

		public async ValueTask<bool> OnTestClassCleanupFailure(Exception exception)
		{
			await using var ctxt = new TestClassRunnerContext<ITestClass, ITestCase>(TestClass, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await OnTestClassCleanupFailure(ctxt, exception);
		}

		public Action? OnTestClassCleanupFailure__Lambda;
		public bool OnTestClassCleanupFailure__Result = true;

		protected override async ValueTask<bool> OnTestClassCleanupFailure(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestClassCleanupFailure(exception: {TypeName(exception)})");

			OnTestClassCleanupFailure__Lambda?.Invoke();

			await base.OnTestClassCleanupFailure(ctxt, exception);

			return OnTestClassCleanupFailure__Result;
		}

		public async ValueTask<bool> OnTestClassFinished(RunSummary summary)
		{
			await using var ctxt = new TestClassRunnerContext<ITestClass, ITestCase>(TestClass, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await OnTestClassFinished(ctxt, summary);
		}

		public Action? OnTestClassFinished__Lambda;
		public bool OnTestClassFinished__Result = true;

		protected override async ValueTask<bool> OnTestClassFinished(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			RunSummary summary)
		{
			Invocations.Add($"OnTestClassFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestClassFinished__Lambda?.Invoke();

			await base.OnTestClassFinished(ctxt, summary);

			return OnTestClassFinished__Result;
		}

		public Action? OnTestClassStarting__Lambda;
		public bool OnTestClassStarting__Result = true;

		public async ValueTask<bool> OnTestClassStarting()
		{
			await using var ctxt = new TestClassRunnerContext<ITestClass, ITestCase>(TestClass, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await OnTestClassStarting(ctxt);
		}

		protected override async ValueTask<bool> OnTestClassStarting(TestClassRunnerContext<ITestClass, ITestCase> ctxt)
		{
			Invocations.Add("OnTestClassStarting");

			OnTestClassStarting__Lambda?.Invoke();

			await base.OnTestClassStarting(ctxt);

			return OnTestClassStarting__Result;
		}

		public RunSummary RunTestMethodAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestMethod(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			ITestMethod? testMethod,
			IReadOnlyCollection<ITestCase> testCases,
			object?[] constructorArguments)
		{
			Invocations.Add($"RunTestMethodAsync(testMethod: {ArgumentFormatter.Format(testMethod?.MethodName)}, constructorArguments: {ArgumentFormatter.Format(constructorArguments)})");

			return new(RunTestMethodAsync__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestClassRunnerContext<ITestClass, ITestCase>(TestClass, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
