using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class TestCollectionRunnerTests
{
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestCollectionStarting()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestCollectionRunner
			{
				OnTestCollectionStarting__Result = false,
				RunTestClassAsync__Result = summary,
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				// RunTestClassAsync
				"OnTestCollectionFinished(summary: { Total = 0 })",
				// OnTestCollectionCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCollectionFinished()
		{
			var runner = new TestableTestCollectionRunner { OnTestCollectionFinished__Result = false };

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: null)",
				"OnTestCollectionFinished(summary: { Total = 0 })",
				// OnTestCollectionCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCollectionCleanupFailure()
		{
			// Need to throw in OnTestCollectionFinished to get OnTestCollectionCleanupFailure to trigger
			var runner = new TestableTestCollectionRunner
			{
				OnTestCollectionCleanupFailure__Result = false,
				OnTestCollectionFinished__Lambda = () => throw new DivideByZeroException(),
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: null)",
				"OnTestCollectionFinished(summary: { Total = 0 })",
				"OnTestCollectionCleanupFailure(exception: typeof(DivideByZeroException))",
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
			var runner = new TestableTestCollectionRunner { RunTestClassAsync__Result = summary };

			var result = await runner.RunAsync();

			Assert.Equal(summary, result);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: null)",
				"OnTestCollectionFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnTestCollectionCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCollectionStarting()
		{
			var runner = new TestableTestCollectionRunner { OnTestCollectionStarting__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: typeof(DivideByZeroException))",
				"OnTestCollectionFinished(summary: { Total = 0 })",
				// OnTestCollectionCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCollectionFinished()
		{
			var runner = new TestableTestCollectionRunner { OnTestCollectionFinished__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: null)",
				"OnTestCollectionFinished(summary: { Total = 0 })",
				"OnTestCollectionCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCollectionCleanupFailure()
		{
			// Need to throw in OnTestCollectionFinished to get OnTestCollectionCleanupFailure to trigger
			var runner = new TestableTestCollectionRunner
			{
				OnTestCollectionCleanupFailure__Lambda = () => throw new DivideByZeroException(),
				OnTestCollectionFinished__Lambda = () => throw new ArgumentException(),
			};

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCollectionStarting",
				"RunTestClassAsync(exception: null)",
				"OnTestCollectionFinished(summary: { Total = 0 })",
				"OnTestCollectionCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageBus.Messages);
			var errorMessage = Assert.IsType<ErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}
	}

	class TestableTestCollectionRunner(
		IReadOnlyCollection<ITestCase>? testCases = null,
		ITestCollection? testCollection = null) :
			TestCollectionRunner<TestCollectionRunnerContext<ITestCollection, ITestCase>, ITestCollection, ITestClass, ITestCase>
	{
		readonly IReadOnlyCollection<ITestCase> testCases = testCases ?? [Mocks.TestCase()];
		readonly ITestCollection testCollection = testCollection ?? Mocks.TestCollection();

		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();

		public Action? OnTestCollectionCleanupFailure__Lambda = null;
		public bool OnTestCollectionCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestCollectionCleanupFailure(
			TestCollectionRunnerContext<ITestCollection, ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestCollectionCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");

			OnTestCollectionCleanupFailure__Lambda?.Invoke();

			return new(OnTestCollectionCleanupFailure__Result);
		}

		public Action? OnTestCollectionFinished__Lambda = null;
		public bool OnTestCollectionFinished__Result = true;

		protected override ValueTask<bool> OnTestCollectionFinished(
			TestCollectionRunnerContext<ITestCollection, ITestCase> ctxt,
			RunSummary summary)
		{
			Invocations.Add($"OnTestCollectionFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestCollectionFinished__Lambda?.Invoke();

			return new(OnTestCollectionFinished__Result);
		}

		public Action? OnTestCollectionStarting__Lambda = null;
		public bool OnTestCollectionStarting__Result = true;

		protected override ValueTask<bool> OnTestCollectionStarting(TestCollectionRunnerContext<ITestCollection, ITestCase> ctxt)
		{
			Invocations.Add("OnTestCollectionStarting");

			OnTestCollectionStarting__Lambda?.Invoke();

			return new(OnTestCollectionStarting__Result);
		}

		public RunSummary RunTestClassAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestClassAsync(
			TestCollectionRunnerContext<ITestCollection, ITestCase> ctxt,
			ITestClass? testClass,
			IReadOnlyCollection<ITestCase> testCases,
			Exception? exception)
		{
			Invocations.Add($"RunTestClassAsync(exception: {TypeName(exception)})");

			return new(RunTestClassAsync__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestCollectionRunnerContext<ITestCollection, ITestCase>(testCollection, testCases, ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
