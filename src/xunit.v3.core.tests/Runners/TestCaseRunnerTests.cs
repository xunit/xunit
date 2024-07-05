using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCaseRunnerTests
{
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestCaseStarting()
		{
			var runner = new TestableTestCaseRunner { OnTestCaseStarting__Result = false };

			await runner.RunAsync();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				// RunTestsAsync
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnTestCaseCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCaseFinished()
		{
			var runner = new TestableTestCaseRunner { OnTestCaseFinished__Result = false };

			await runner.RunAsync();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: null)",
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnTestCaseCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCaseCleanupFailure()
		{
			// Need to throw in OnTestCaseFinished to get OnTestCaseCleanupFailure to trigger
			var runner = new TestableTestCaseRunner
			{
				OnTestCaseCleanupFailure__Result = false,
				OnTestCaseFinished__Lambda = _ => throw new DivideByZeroException(),
			};

			await runner.RunAsync();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: null)",
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnTestCaseCleanupFailure(exception: typeof(DivideByZeroException))",
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
			var runner = new TestableTestCaseRunner { RunTestsAsync__Result = summary };

			var result = await runner.RunAsync();

			Assert.Equal(result, summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: null)",
				"OnTestCaseFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnTestCaseCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCaseStarting()
		{
			var runner = new TestableTestCaseRunner { OnTestCaseStarting__Lambda = _ => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: typeof(DivideByZeroException))",
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnTestCaseCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCaseFinished()
		{
			var runner = new TestableTestCaseRunner { OnTestCaseFinished__Lambda = _ => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: null)",
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnTestCaseCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestCaseCleanupFailure()
		{
			// Need to throw in OnTestCaseFinished to get OnTestCaseCleanupFailure to trigger
			var runner = new TestableTestCaseRunner
			{
				OnTestCaseFinished__Lambda = _ => throw new ArgumentException(),
				OnTestCaseCleanupFailure__Lambda = _ => throw new DivideByZeroException(),
			};

			await runner.RunAsync();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestsAsync(exception: null)",
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnTestCaseCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageBus.Messages);
			var errorMessage = Assert.IsType<_ErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}
	}

	class TestableTestCaseRunner(_ITestCase? testCase = null) :
		TestCaseRunner<TestCaseRunnerContext<_ITestCase>, _ITestCase>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		public readonly _ITestCase TestCase = testCase ?? Mocks.TestCase();
		public readonly CancellationTokenSource TokenSource = new();

		public Action<TestCaseRunnerContext<_ITestCase>>? OnTestCaseCleanupFailure__Lambda;
		public bool OnTestCaseCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestCaseCleanupFailure(
			TestCaseRunnerContext<_ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestCaseCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");

			OnTestCaseCleanupFailure__Lambda?.Invoke(ctxt);

			return new(OnTestCaseCleanupFailure__Result);
		}

		public RunSummary? OnTestCaseFinished_Summary;
		public Action<TestCaseRunnerContext<_ITestCase>>? OnTestCaseFinished__Lambda;
		public bool OnTestCaseFinished__Result = true;

		protected override ValueTask<bool> OnTestCaseFinished(
			TestCaseRunnerContext<_ITestCase> ctxt,
			RunSummary summary)
		{
			Invocations.Add($"OnTestCaseFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestCaseFinished_Summary = summary;
			OnTestCaseFinished__Lambda?.Invoke(ctxt);

			return new(OnTestCaseFinished__Result);
		}

		public Action<TestCaseRunnerContext<_ITestCase>>? OnTestCaseStarting__Lambda;
		public bool OnTestCaseStarting__Result = true;

		protected override ValueTask<bool> OnTestCaseStarting(TestCaseRunnerContext<_ITestCase> ctxt)
		{
			Invocations.Add("OnTestCaseStarting");

			OnTestCaseStarting__Lambda?.Invoke(ctxt);

			return new(OnTestCaseStarting__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestCaseRunnerContext<_ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		public RunSummary RunTestsAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestsAsync(
			TestCaseRunnerContext<_ITestCase> ctxt,
			Exception? exception)
		{
			Invocations.Add($"RunTestsAsync(exception: {TypeName(exception)})");

			return new(RunTestsAsync__Result);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
