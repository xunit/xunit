using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class TestAssemblyRunnerTests
{
	public class Cancellation
	{
		[Fact]
		public static async ValueTask OnTestAssemblyStarting()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestAssemblyRunner
			{
				OnTestAssemblyStarting__Result = false,
				RunTestCollectionAsync__Result = summary,
			};

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				// RunTestCollectionAsync
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}

		[Fact]
		public static async ValueTask OnTestAssemblyFinished()
		{
			var runner = new TestableTestAssemblyRunner { OnTestAssemblyFinished__Result = false };

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}

		[Fact]
		public static async ValueTask OnTestAssemblyCleanupFailure()
		{
			// Need to throw in OnTestAssemblyFinished to get OnTestAssemblyCleanupFailure to trigger
			var runner = new TestableTestAssemblyRunner
			{
				OnTestAssemblyFinished__Lambda = () => throw new DivideByZeroException(),
				OnTestAssemblyCleanupFailure__Result = false,
			};

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.True(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}
	}

	public class ExceptionHandling
	{
		[Fact]
		public static async ValueTask NoExceptions()
		{
			var summary = new RunSummary { Total = 9, Failed = 2, Skipped = 1, NotRun = 3, Time = 21.12m };
			var runner = new TestableTestAssemblyRunner { RunTestCollectionAsync__Result = summary };

			var result = await runner.RunAsync();

			// Can't verify time because it's overwritten with clock time
			Assert.Equivalent(new { Total = 9, Failed = 2, Skipped = 1, NotRun = 3 }, result);
			Assert.NotNull(runner.CancellationTokenSource);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}

		[Fact]
		public static async ValueTask OnTestAssemblyStarting()
		{
			var runner = new TestableTestAssemblyRunner { OnTestAssemblyStarting__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: typeof(DivideByZeroException))",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}

		[Fact]
		public static async ValueTask OnTestAssemblyFinished()
		{
			var runner = new TestableTestAssemblyRunner { OnTestAssemblyFinished__Lambda = () => throw new DivideByZeroException() };

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
			Assert.Empty(runner.MessageSink.Messages);
		}

		[Fact]
		public static async ValueTask OnTestAssemblyCleanupFailure()
		{
			// Need to throw in OnTestAssemblyFinished to get OnTestAssemblyCleanupFailure to trigger
			var runner = new TestableTestAssemblyRunner
			{
				OnTestAssemblyCleanupFailure__Lambda = () => throw new DivideByZeroException(),
				OnTestAssemblyFinished__Lambda = () => throw new ArgumentException(),
			};

			await runner.RunAsync();

			Assert.NotNull(runner.CancellationTokenSource);
			Assert.False(runner.CancellationTokenSource.IsCancellationRequested);
			Assert.Equal(new[]
			{
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			var message = Assert.Single(runner.MessageSink.Messages);
			var errorMessage = Assert.IsType<_ErrorMessage>(message);
			Assert.Equal(new[] { -1 }, errorMessage.ExceptionParentIndices);
			Assert.Equal(new[] { "System.DivideByZeroException" }, errorMessage.ExceptionTypes);
			Assert.Equal(new[] { "Attempted to divide by zero." }, errorMessage.Messages);
			Assert.NotEmpty(errorMessage.StackTraces.Single()!);
		}
	}

	public class OrderTestCollections
	{
		[Fact]
		public static async ValueTask DefaultTestOrdering()
		{
			var collection1 = Mocks.TestCollection(testCollectionDisplayName: "1", uniqueID: "collection-1");
			var testCase1a = testCaseForCollection(collection1, "1a");
			var testCase1b = testCaseForCollection(collection1, "1b");
			var collection2 = Mocks.TestCollection(testCollectionDisplayName: "2", uniqueID: "collection-2");
			var testCase2a = testCaseForCollection(collection2, "2a");
			var testCase2b = testCaseForCollection(collection2, "2b");
			var runner = new TestableTestAssemblyRunner(testCases: [testCase1a, testCase2a, testCase2b, testCase1b]);

			await runner.RunAsync();

			Assert.Equal([
				"OnTestAssemblyStarting",
				"RunTestCollectionAsync(testCollection: '1', testCases: ['1a','1b'], exception: null)",
				"RunTestCollectionAsync(testCollection: '2', testCases: ['2a','2b'], exception: null)",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
			], runner.Invocations);

			static _ITestCase testCaseForCollection(
				_ITestCollection testCollection,
				string testCaseDisplayName) =>
					Mocks.TestCase(testMethod: Mocks.TestMethod(testClass: Mocks.TestClass(testCollection: testCollection)), testCaseDisplayName: testCaseDisplayName);
		}
	}

	class TestableTestAssemblyRunner(
		IReadOnlyCollection<_ITestCase>? testCases = null,
		_ITestAssembly? TestAssembly = null) :
			TestAssemblyRunner<TestAssemblyRunnerContext<_ITestAssembly, _ITestCase>, _ITestAssembly, _ITestCollection, _ITestCase>
	{
		readonly IReadOnlyCollection<_ITestCase> testCases = testCases ?? [Mocks.TestCase()];
		readonly _ITestAssembly TestAssembly = TestAssembly ?? Mocks.TestAssembly();

		public CancellationTokenSource? CancellationTokenSource;  // Gets set by OnTestAssemblyStarting
		public readonly _ITestFrameworkExecutionOptions ExecutionOptions = TestData.TestFrameworkExecutionOptions();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();

		public Action? OnTestAssemblyCleanupFailure__Lambda = null;
		public bool OnTestAssemblyCleanupFailure__Result = true;

		protected override ValueTask<bool> OnTestAssemblyCleanupFailure(
			TestAssemblyRunnerContext<_ITestAssembly, _ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestAssemblyCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");

			OnTestAssemblyCleanupFailure__Lambda?.Invoke();

			return new(OnTestAssemblyCleanupFailure__Result);
		}

		public Action? OnTestAssemblyFinished__Lambda = null;
		public bool OnTestAssemblyFinished__Result = true;

		protected override ValueTask<bool> OnTestAssemblyFinished(
			TestAssemblyRunnerContext<_ITestAssembly, _ITestCase> ctxt,
			RunSummary summary)
		{
			// We know that we record clock time, so we're going to zero out the time in the summary, so that we get
			// a predictable printed value
			summary.Time = 0m;

			Invocations.Add($"OnTestAssemblyFinished(summary: {ArgumentFormatter.Format(summary)})");

			OnTestAssemblyFinished__Lambda?.Invoke();

			return new(OnTestAssemblyFinished__Result);
		}

		public Action? OnTestAssemblyStarting__Lambda = null;
		public bool OnTestAssemblyStarting__Result = true;

		protected override ValueTask<bool> OnTestAssemblyStarting(TestAssemblyRunnerContext<_ITestAssembly, _ITestCase> ctxt)
		{
			CancellationTokenSource = ctxt.CancellationTokenSource;

			Invocations.Add("OnTestAssemblyStarting");

			OnTestAssemblyStarting__Lambda?.Invoke();

			return new(OnTestAssemblyStarting__Result);
		}

		public RunSummary RunTestCollectionAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestCollectionAsync(
			TestAssemblyRunnerContext<_ITestAssembly, _ITestCase> ctxt,
			_ITestCollection testCollection,
			IReadOnlyCollection<_ITestCase> testCases,
			Exception? exception)
		{
			Invocations.Add($"RunTestCollectionAsync(testCollection: '{testCollection.TestCollectionDisplayName}', testCases: [{string.Join(",", testCases.Select(tc => "'" + tc.TestCaseDisplayName + "'"))}], exception: {TypeName(exception)})");

			return new(RunTestCollectionAsync__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestAssemblyRunnerContext<_ITestAssembly, _ITestCase>(TestAssembly, testCases, MessageSink, ExecutionOptions);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
