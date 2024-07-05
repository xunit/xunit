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
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: null)",
				"OnTestClassFinished(summary: { Total = 0 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: null)",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: null)",
				"OnTestClassFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: typeof(DivideByZeroException))",
				"OnTestClassFinished(summary: { Total = 0 })",
				// OnTestClassCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: null)",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
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
				"RunTestMethodAsync(testMethod: \"\", constructorArguments: [], exception: null)",
				"OnTestClassFinished(summary: { Total = 0 })",
				"OnTestClassCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageBus.Messages);
			var errorMessage = Assert.IsType<ErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
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

		public Action? OnTestClassCleanupFailure__Lambda;
		public bool OnTestClassCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestClassCleanupFailure(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestClassCleanupFailure(exception: {TypeName(exception)})");

			OnTestClassCleanupFailure__Lambda?.Invoke();

			return new(OnTestClassCleanupFailure__Result);
		}

		public Action? OnTestClassFinished__Lambda;
		public bool OnTestClassFinished__Result = true;

		protected override ValueTask<bool> OnTestClassFinished(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			RunSummary summary)
		{
			Invocations.Add($"OnTestClassFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestClassFinished__Lambda?.Invoke();

			return new(OnTestClassFinished__Result);
		}

		public Action? OnTestClassStarting__Lambda;
		public bool OnTestClassStarting__Result = true;

		protected override ValueTask<bool> OnTestClassStarting(TestClassRunnerContext<ITestClass, ITestCase> ctxt)
		{
			Invocations.Add("OnTestClassStarting");

			OnTestClassStarting__Lambda?.Invoke();

			return new(OnTestClassStarting__Result);
		}

		public RunSummary RunTestMethodAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestMethodAsync(
			TestClassRunnerContext<ITestClass, ITestCase> ctxt,
			ITestMethod? testMethod,
			IReadOnlyCollection<ITestCase> testCases,
			object?[] constructorArguments,
			Exception? exception)
		{
			Invocations.Add($"RunTestMethodAsync(testMethod: {ArgumentFormatter.Format(testMethod?.MethodName)}, constructorArguments: {ArgumentFormatter.Format(constructorArguments)}, exception: {TypeName(exception)})");

			return new(RunTestMethodAsync__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestClassRunnerContext<ITestClass, ITestCase>(TestClass, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
