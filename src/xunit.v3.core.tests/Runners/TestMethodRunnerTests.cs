using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestMethodRunnerTests
{
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestMethodStarting()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestMethodRunner
			{
				OnTestMethodStarting__Result = false,
				RunTestCaseAsync__Result = summary,
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				// RunTestCaseAsync
				"OnTestMethodFinished(summary: { Total = 0 })",
				// OnTestMethodCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestMethodFinished()
		{
			var runner = new TestableTestMethodRunner { OnTestMethodFinished__Result = false };

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: null)",
				"OnTestMethodFinished(summary: { Total = 0 })",
				// OnTestMethodCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestMethodCleanupFailure()
		{
			// Need to throw in OnTestMethodFinished to get OnTestMethodCleanupFailure to trigger
			var runner = new TestableTestMethodRunner
			{
				OnTestMethodCleanupFailure__Result = false,
				OnTestMethodFinished__Lambda = () => throw new DivideByZeroException(),
			};

			await runner.RunAsync();

			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: null)",
				"OnTestMethodFinished(summary: { Total = 0 })",
				"OnTestMethodCleanupFailure(exception: typeof(DivideByZeroException))",
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
			var runner = new TestableTestMethodRunner { RunTestCaseAsync__Result = summary };

			var result = await runner.RunAsync();

			Assert.Equal(summary, result);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: null)",
				"OnTestMethodFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnTestMethodCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestMethodStarting()
		{
			var runner = new TestableTestMethodRunner { OnTestMethodStarting__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: typeof(DivideByZeroException))",
				"OnTestMethodFinished(summary: { Total = 0 })",
				// OnTestMethodCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestMethodFinished()
		{
			var runner = new TestableTestMethodRunner { OnTestMethodFinished__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: null)",
				"OnTestMethodFinished(summary: { Total = 0 })",
				"OnTestMethodCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageBus.Messages);
		}

		[Fact]
		public static async ValueTask OnTestMethodCleanupFailure()
		{
			// Need to throw in OnTestMethodFinished to get OnTestMethodCleanupFailure to trigger
			var runner = new TestableTestMethodRunner
			{
				OnTestMethodCleanupFailure__Lambda = () => throw new DivideByZeroException(),
				OnTestMethodFinished__Lambda = () => throw new ArgumentException(),
			};

			await runner.RunAsync();

			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestMethodStarting",
				"RunTestCaseAsync(exception: null)",
				"OnTestMethodFinished(summary: { Total = 0 })",
				"OnTestMethodCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageBus.Messages);
			var errorMessage = Assert.IsType<ErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}
	}

	class TestableTestMethodRunner(ITestCase? testCase = null) :
		TestMethodRunner<TestMethodRunnerContext<ITestMethod, ITestCase>, ITestMethod, ITestCase>
	{
		readonly ITestCase testCase = testCase ?? Mocks.TestCase();

		public readonly ExceptionAggregator Aggregator = new();
		public readonly CancellationTokenSource CancellationTokenSource = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		ITestMethod TestMethod => Guard.ArgumentNotNull(testCase.TestMethod);

		public Action? OnTestMethodCleanupFailure__Lambda = null;
		public bool OnTestMethodCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestMethodCleanupFailure(
			TestMethodRunnerContext<ITestMethod, ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestMethodCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");

			OnTestMethodCleanupFailure__Lambda?.Invoke();

			return new(OnTestMethodCleanupFailure__Result);
		}

		public Action? OnTestMethodFinished__Lambda = null;
		public bool OnTestMethodFinished__Result = true;

		protected override ValueTask<bool> OnTestMethodFinished(
			TestMethodRunnerContext<ITestMethod, ITestCase> ctxt,
			RunSummary summary)
		{
			Invocations.Add($"OnTestMethodFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestMethodFinished__Lambda?.Invoke();

			return new(OnTestMethodFinished__Result);
		}

		public Action? OnTestMethodStarting__Lambda = null;
		public bool OnTestMethodStarting__Result = true;

		protected override ValueTask<bool> OnTestMethodStarting(TestMethodRunnerContext<ITestMethod, ITestCase> ctxt)
		{
			Invocations.Add("OnTestMethodStarting");

			OnTestMethodStarting__Lambda?.Invoke();

			return new(OnTestMethodStarting__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestMethodRunnerContext<ITestMethod, ITestCase>(TestMethod, [testCase], ExplicitOption.Off, MessageBus, Aggregator, CancellationTokenSource);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		public RunSummary RunTestCaseAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestCaseAsync(
			TestMethodRunnerContext<ITestMethod, ITestCase> ctxt,
			ITestCase testCase,
			Exception? exception)
		{
			Invocations.Add($"RunTestCaseAsync(exception: {TypeName(exception)})");

			return new(RunTestCaseAsync__Result);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
