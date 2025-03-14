using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestCaseRunnerBaseTests
{
	public class Messages
	{
		[Fact]
		public async ValueTask OnError()
		{
			var runner = new TestableTestCaseRunnerBase();
			var ex = Record.Exception(ThrowException);

			await runner.OnError(ex!);

			var message = Assert.Single(runner.MessageBus.Messages);
			var error = Assert.IsAssignableFrom<IErrorMessage>(message);

			Assert.Equal(-1, error.ExceptionParentIndices.Single());
			Assert.Equal(typeof(DivideByZeroException).FullName, error.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", error.Messages.Single());
			Assert.NotEmpty(error.StackTraces.Single()!);
		}

		[Fact]
		public async ValueTask OnTestCaseCleanupFailure()
		{
			var runner = new TestableTestCaseRunnerBase();
			var ex = Record.Exception(ThrowException);

			await runner.OnTestCaseCleanupFailure(ex!);

			var message = Assert.Single(runner.MessageBus.Messages);
			var failure = Assert.IsAssignableFrom<ITestCaseCleanupFailure>(message);

			VerifyTestCaseMessage(failure);
			Assert.Equal(-1, failure.ExceptionParentIndices.Single());
			Assert.Equal(typeof(DivideByZeroException).FullName, failure.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", failure.Messages.Single());
			Assert.NotEmpty(failure.StackTraces.Single()!);
		}

		[Fact]
		public async ValueTask OnTestCaseFinished()
		{
			var runner = new TestableTestCaseRunnerBase();
			var summary = new RunSummary { Total = 2112, Failed = 42, Skipped = 21, NotRun = 9, Time = 123.45m };

			await runner.OnTestCaseFinished(summary);

			var message = Assert.Single(runner.MessageBus.Messages);
			var finished = Assert.IsAssignableFrom<ITestCaseFinished>(message);

			VerifyTestCaseMessage(finished);
			Assert.Equal(123.45m, finished.ExecutionTime);
			Assert.Equal(42, finished.TestsFailed);
			Assert.Equal(9, finished.TestsNotRun);
			Assert.Equal(21, finished.TestsSkipped);
			Assert.Equal(2112, finished.TestsTotal);
		}

		[Fact]
		public async ValueTask OnTestCaseStarting()
		{
			var testCase = Mocks.TestCase(
				@explicit: true,
				skipReason: "Skipping",
				sourceFilePath: "/path/to/source.cs",
				sourceLineNumber: 42
			);
			var runner = new TestableTestCaseRunnerBase(testCase);

			await runner.OnTestCaseStarting();

			var message = Assert.Single(runner.MessageBus.Messages);
			var starting = Assert.IsAssignableFrom<ITestCaseStarting>(message);

			VerifyTestCaseMessage(starting);
			Assert.True(starting.Explicit);
			Assert.Equal("Skipping", starting.SkipReason);
			Assert.Equal("/path/to/source.cs", starting.SourceFilePath);
			Assert.Equal(42, starting.SourceLineNumber);
			Assert.Equal("test-case-display-name", starting.TestCaseDisplayName);
			Assert.Equal("test-class-name", starting.TestClassName);
			Assert.Equal("test-class-namespace", starting.TestClassNamespace);
			Assert.Equal("test-class-simple-name", starting.TestClassSimpleName);
			Assert.Equal("test-method", starting.TestMethodName);
			Assert.Equivalent(TestData.DefaultTraits, starting.Traits);
		}

		static void ThrowException() =>
			throw new DivideByZeroException();

		static void VerifyTestCaseMessage(ITestCaseMessage message)
		{
			Assert.Equal("assembly-id", message.AssemblyUniqueID);
			Assert.Equal("test-collection-id", message.TestCollectionUniqueID);
			Assert.Equal("test-class-id", message.TestClassUniqueID);
			Assert.Equal("test-method-id", message.TestMethodUniqueID);
			Assert.Equal("test-case-id", message.TestCaseUniqueID);
		}
	}

	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnError()
		{
			// Need to record an exception into the aggregator for OnTestCaseFinished to trigger OnTestCaseCleanupFailure
			var runner = new TestableTestCaseRunnerBase
			{
				OnError__Result = false,
				OnTestCaseFinished__Lambda = _ => throw new DivideByZeroException(),
			};

			await runner.Run();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnError(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseStarting()
		{
			var runner = new TestableTestCaseRunnerBase { OnTestCaseStarting__Result = false };

			await runner.Run();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				// RunTestCase
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseFinished()
		{
			var runner = new TestableTestCaseRunnerBase { OnTestCaseFinished__Result = false };

			await runner.Run();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseCleanupFailure()
		{
			// Need to record an exception into the aggregator for OnTestCaseFinished to trigger OnTestCaseCleanupFailure
			var runner = new TestableTestCaseRunnerBase { OnTestCaseCleanupFailure__Result = false };
			runner.OnTestCaseFinished__Lambda = _ => runner.Aggregator.Add(new DivideByZeroException());

			await runner.Run();

			Assert.True(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				"OnTestCaseCleanupFailure(exception: typeof(DivideByZeroException))",
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnError
			}, runner.Invocations);
		}
	}

	public class ExceptionHandling
	{
		[Fact]
		public static async ValueTask NoExceptions()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestCaseRunnerBase { RunTestCase__Result = summary };

			var result = await runner.Run();

			Assert.Equal(result, summary);
			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12 })",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseStarting()
		{
			var runner = new TestableTestCaseRunnerBase { OnTestCaseStarting__Lambda = _ => throw new DivideByZeroException() };

			await runner.Run();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: typeof(DivideByZeroException))",
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 0 })",
				// OnError
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseFinished()
		{
			var runner = new TestableTestCaseRunnerBase { OnTestCaseFinished__Lambda = _ => throw new DivideByZeroException() };

			await runner.Run();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				// OnTestCaseCleanupFailure
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnError(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}

		[Fact]
		public static async ValueTask OnTestCaseCleanupFailure()
		{
			// Need to record an exception into the aggregator for OnTestCaseFinished to trigger OnTestCaseCleanupFailure
			var runner = new TestableTestCaseRunnerBase { OnTestCaseCleanupFailure__Lambda = _ => throw new DivideByZeroException() };
			runner.OnTestCaseFinished__Lambda = _ => runner.Aggregator.Add(new ArgumentException());

			await runner.Run();

			Assert.False(runner.TokenSource.IsCancellationRequested);
			Assert.False(runner.Aggregator.HasExceptions);
			Assert.Equal(new[]
			{
				"OnTestCaseStarting",
				"RunTestCase(testCase: 'test-case-display-name', exception: null)",
				"OnTestCaseCleanupFailure(exception: typeof(ArgumentException))",
				"OnTestCaseFinished(summary: { Total = 0 })",
				"OnError(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
		}
	}

	class TestableTestCaseRunnerBase(ITestCase? testCase = null) :
		TestCaseRunnerBase<TestCaseRunnerBaseContext<ITestCase>, ITestCase>
	{
		public readonly ExceptionAggregator Aggregator = new();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageBus MessageBus = new();
		public readonly ITestCase TestCase = testCase ?? Mocks.TestCase();
		public readonly CancellationTokenSource TokenSource = new();

		public async ValueTask<bool> OnError(Exception exception)
		{
			await using var ctxt = new TestCaseRunnerBaseContext<ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await OnError(ctxt, exception);
		}

		public bool OnError__Result = true;

		protected override async ValueTask<bool> OnError(
			TestCaseRunnerBaseContext<ITestCase> ctxt,
			Exception exception)
		{
			try
			{
				await base.OnError(ctxt, exception);

				return OnError__Result;
			}
			finally
			{
				Invocations.Add($"OnError(exception: {TypeName(exception)})");
			}
		}

		public async ValueTask<bool> OnTestCaseCleanupFailure(Exception exception)
		{
			await using var ctxt = new TestCaseRunnerBaseContext<ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await OnTestCaseCleanupFailure(ctxt, exception);
		}

		public Action<TestCaseRunnerBaseContext<ITestCase>>? OnTestCaseCleanupFailure__Lambda;
		public bool OnTestCaseCleanupFailure__Result = true;

		protected override async ValueTask<bool> OnTestCaseCleanupFailure(
			TestCaseRunnerBaseContext<ITestCase> ctxt,
			Exception exception)
		{
			try
			{
				OnTestCaseCleanupFailure__Lambda?.Invoke(ctxt);

				await base.OnTestCaseCleanupFailure(ctxt, exception);

				return OnTestCaseCleanupFailure__Result;
			}
			finally
			{
				Invocations.Add($"OnTestCaseCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");
			}
		}

		public async ValueTask<bool> OnTestCaseFinished(RunSummary summary)
		{
			await using var ctxt = new TestCaseRunnerBaseContext<ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await OnTestCaseFinished(ctxt, summary);
		}

		public RunSummary? OnTestCaseFinished_Summary;
		public Action<TestCaseRunnerBaseContext<ITestCase>>? OnTestCaseFinished__Lambda;
		public bool OnTestCaseFinished__Result = true;

		protected override async ValueTask<bool> OnTestCaseFinished(
			TestCaseRunnerBaseContext<ITestCase> ctxt,
			RunSummary summary)
		{
			try
			{
				OnTestCaseFinished_Summary = summary;
				OnTestCaseFinished__Lambda?.Invoke(ctxt);

				await base.OnTestCaseFinished(ctxt, summary);

				return OnTestCaseFinished__Result;
			}
			finally
			{
				Invocations.Add($"OnTestCaseFinished(summary: {ArgumentFormatter.Format(summary)})");
			}
		}

		public async ValueTask<bool> OnTestCaseStarting()
		{
			await using var ctxt = new TestCaseRunnerBaseContext<ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await OnTestCaseStarting(ctxt);
		}

		public Action<TestCaseRunnerBaseContext<ITestCase>>? OnTestCaseStarting__Lambda;
		public bool OnTestCaseStarting__Result = true;

		protected override async ValueTask<bool> OnTestCaseStarting(TestCaseRunnerBaseContext<ITestCase> ctxt)
		{
			try
			{
				OnTestCaseStarting__Lambda?.Invoke(ctxt);

				await base.OnTestCaseStarting(ctxt);

				return OnTestCaseStarting__Result;
			}
			finally
			{
				Invocations.Add("OnTestCaseStarting");
			}
		}

		public async ValueTask<RunSummary> Run()
		{
			await using var ctxt = new TestCaseRunnerBaseContext<ITestCase>(TestCase, ExplicitOption.Off, MessageBus, Aggregator, TokenSource);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		public RunSummary RunTestCase__Result = new();

		protected override ValueTask<RunSummary> RunTestCase(
			TestCaseRunnerBaseContext<ITestCase> ctxt,
			Exception? exception)
		{
			try
			{
				return new(RunTestCase__Result);
			}
			finally
			{
				Invocations.Add($"RunTestCase(testCase: '{ctxt.TestCase.TestCaseDisplayName}', exception: {TypeName(exception)})");
			}
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
