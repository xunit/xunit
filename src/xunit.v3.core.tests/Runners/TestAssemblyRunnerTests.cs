using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public static class TestAssemblyRunnerTests
{
	public class Messages
	{
		[Fact]
		public async ValueTask OnTestAssemblyCleanupFailure()
		{
			var runner = new TestableTestAssemblyRunner();
			var ex = Record.Exception(ThrowException);

			await runner.OnTestAssemblyCleanupFailure(ex!);

			var message = Assert.Single(runner.MessageSink.Messages);
			var failure = Assert.IsAssignableFrom<ITestAssemblyCleanupFailure>(message);

			VerifyTestAssemblyMessage(failure);
			Assert.Equal(-1, failure.ExceptionParentIndices.Single());
			Assert.Equal(typeof(DivideByZeroException).FullName, failure.ExceptionTypes.Single());
			Assert.Equal("Attempted to divide by zero.", failure.Messages.Single());
			Assert.NotEmpty(failure.StackTraces.Single()!);
		}

		[Fact]
		public async ValueTask OnTestAssemblyFinished()
		{
			var runner = new TestableTestAssemblyRunner();
			var summary = new RunSummary { Total = 2112, Failed = 42, Skipped = 21, NotRun = 9, Time = 123.45m };

			await runner.OnTestAssemblyFinished(summary);

			var message = Assert.Single(runner.MessageSink.Messages);
			var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);

			VerifyTestAssemblyMessage(finished);
			Assert.Equal(123.45m, finished.ExecutionTime);
			Assert.NotEqual(default, finished.FinishTime);
			Assert.Equal(42, finished.TestsFailed);
			Assert.Equal(9, finished.TestsNotRun);
			Assert.Equal(21, finished.TestsSkipped);
			Assert.Equal(2112, finished.TestsTotal);
		}

		[Fact]
		public async ValueTask OnTestAssemblyStarting()
		{
			var runner = new TestableTestAssemblyRunner();

			await runner.OnTestAssemblyStarting();

			var message = Assert.Single(runner.MessageSink.Messages);
			var starting = Assert.IsAssignableFrom<ITestAssemblyStarting>(message);

			VerifyTestAssemblyMessage(starting);
			Assert.Equal("test-assembly", starting.AssemblyName);
			Assert.Equal("./test-assembly.dll", starting.AssemblyPath);
			Assert.Null(starting.ConfigFilePath);
			Assert.NotNull(starting.Seed);  // We don't know what the seed will be, we just know it will have one
			Assert.NotEqual(default, starting.StartTime);
			Assert.Null(starting.TargetFramework);  // Can be overriden in the context as needed, defaults to null
			Assert.Matches($"^{IntPtr.Size * 8}-bit \\({Regex.Escape(RuntimeInformation.ProcessArchitecture.ToDisplayName())}\\) {Regex.Escape(RuntimeInformation.FrameworkDescription)}$", starting.TestEnvironment);
			Assert.Equal("Stub Testing Framework", starting.TestFrameworkDisplayName);
			Assert.Equivalent(TestData.DefaultTraits, starting.Traits);
		}

		static void ThrowException() =>
			throw new DivideByZeroException();

		static void VerifyTestAssemblyMessage(ITestAssemblyMessage message) =>
			Assert.Equal("assembly-id", message.AssemblyUniqueID);
	}

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
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'])",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
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
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'])",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
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
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'])",
				"OnTestAssemblyFinished(summary: { Total = 9, Failed = 2, Skipped = 1, NotRun = 3 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
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
				"FailTestCollection(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'], exception: typeof(DivideByZeroException))",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				// OnTestAssemblyCleanupFailure
			}, runner.Invocations);
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
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'])",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(DivideByZeroException))",
			}, runner.Invocations);
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
				"RunTestCollectionAsync(testCollection: 'test-collection-display-name', testCases: ['test-case-display-name'])",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
				"OnTestAssemblyCleanupFailure(exception: typeof(ArgumentException))",
			}, runner.Invocations);
			Assert.Collection(
				runner.MessageSink.Messages,
				message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
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
				"RunTestCollectionAsync(testCollection: '1', testCases: ['1a','1b'])",
				"RunTestCollectionAsync(testCollection: '2', testCases: ['2a','2b'])",
				"OnTestAssemblyFinished(summary: { Total = 0 })",
			], runner.Invocations);

			static ITestCase testCaseForCollection(
				ITestCollection testCollection,
				string testCaseDisplayName) =>
					Mocks.TestCase(testMethod: Mocks.TestMethod(testClass: Mocks.TestClass(testCollection: testCollection)), testCaseDisplayName: testCaseDisplayName);
		}
	}

	class TestableTestAssemblyRunner(
		IReadOnlyCollection<ITestCase>? testCases = null,
		ITestAssembly? TestAssembly = null) :
			TestAssemblyRunner<TestAssemblyRunnerContext<ITestAssembly, ITestCase>, ITestAssembly, ITestCollection, ITestCase>
	{
		readonly IReadOnlyCollection<ITestCase> testCases = testCases ?? [Mocks.TestCase()];
		readonly ITestAssembly TestAssembly = TestAssembly ?? Mocks.TestAssembly();

		public CancellationTokenSource? CancellationTokenSource;  // Gets set by OnTestAssemblyStarting
		public readonly ITestFrameworkExecutionOptions ExecutionOptions = TestData.TestFrameworkExecutionOptions();
		public readonly List<string> Invocations = [];
		public readonly SpyMessageSink MessageSink = SpyMessageSink.Capture();

		public RunSummary FailTestCollection__Result = new();

		protected override ValueTask<RunSummary> FailTestCollection(
			TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt,
			ITestCollection testCollection,
			IReadOnlyCollection<ITestCase> testCases,
			Exception? exception)
		{
			Invocations.Add($"FailTestCollection(testCollection: '{testCollection.TestCollectionDisplayName}', testCases: [{string.Join(",", testCases.Select(tc => "'" + tc.TestCaseDisplayName + "'"))}], exception: {TypeName(exception)})");

			return new(FailTestCollection__Result);
		}

		protected override ValueTask<string> GetTestFrameworkDisplayName(TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt) =>
			new("Stub Testing Framework");

		public async ValueTask<bool> OnTestAssemblyCleanupFailure(Exception exception)
		{
			await using var ctxt = new TestAssemblyRunnerContext<ITestAssembly, ITestCase>(TestAssembly, testCases, MessageSink, ExecutionOptions, default);
			await ctxt.InitializeAsync();

			return await OnTestAssemblyCleanupFailure(ctxt, exception);
		}

		public Action? OnTestAssemblyCleanupFailure__Lambda = null;
		public bool OnTestAssemblyCleanupFailure__Result = true;

		protected override async ValueTask<bool> OnTestAssemblyCleanupFailure(
			TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt,
			Exception exception)
		{
			Invocations.Add($"OnTestAssemblyCleanupFailure(exception: typeof({ArgumentFormatter.FormatTypeName(exception.GetType())}))");

			OnTestAssemblyCleanupFailure__Lambda?.Invoke();

			await base.OnTestAssemblyCleanupFailure(ctxt, exception);

			return OnTestAssemblyCleanupFailure__Result;
		}

		public async ValueTask<bool> OnTestAssemblyFinished(RunSummary summary)
		{
			await using var ctxt = new TestAssemblyRunnerContext<ITestAssembly, ITestCase>(TestAssembly, testCases, MessageSink, ExecutionOptions, default);
			await ctxt.InitializeAsync();

			return await OnTestAssemblyFinished(ctxt, summary);
		}

		public Action? OnTestAssemblyFinished__Lambda = null;
		public bool OnTestAssemblyFinished__Result = true;

		protected override async ValueTask<bool> OnTestAssemblyFinished(
			TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt,
			RunSummary summary)
		{
			// Temporarily replace time with 0 so we don't have to worry about clock time from RunAsync in the argument format
			var time = summary.Time;
			summary.Time = 0;
			Invocations.Add($"OnTestAssemblyFinished(summary: {ArgumentFormatter.Format(summary)})");
			summary.Time = time;

			OnTestAssemblyFinished__Lambda?.Invoke();

			await base.OnTestAssemblyFinished(ctxt, summary);

			return OnTestAssemblyFinished__Result;
		}

		public async ValueTask<bool> OnTestAssemblyStarting()
		{
			await using var ctxt = new TestAssemblyRunnerContext<ITestAssembly, ITestCase>(TestAssembly, testCases, MessageSink, ExecutionOptions, default);
			await ctxt.InitializeAsync();

			return await OnTestAssemblyStarting(ctxt);
		}

		public Action? OnTestAssemblyStarting__Lambda = null;
		public bool OnTestAssemblyStarting__Result = true;

		protected override async ValueTask<bool> OnTestAssemblyStarting(TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt)
		{
			CancellationTokenSource = ctxt.CancellationTokenSource;

			Invocations.Add("OnTestAssemblyStarting");

			OnTestAssemblyStarting__Lambda?.Invoke();

			await base.OnTestAssemblyStarting(ctxt);

			return OnTestAssemblyStarting__Result;
		}

		public RunSummary RunTestCollectionAsync__Result = new();

		protected override ValueTask<RunSummary> RunTestCollection(
			TestAssemblyRunnerContext<ITestAssembly, ITestCase> ctxt,
			ITestCollection testCollection,
			IReadOnlyCollection<ITestCase> testCases)
		{
			Invocations.Add($"RunTestCollectionAsync(testCollection: '{testCollection.TestCollectionDisplayName}', testCases: [{string.Join(",", testCases.Select(tc => "'" + tc.TestCaseDisplayName + "'"))}])");

			return new(RunTestCollectionAsync__Result);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new TestAssemblyRunnerContext<ITestAssembly, ITestCase>(TestAssembly, testCases, MessageSink, ExecutionOptions, default);
			await ctxt.InitializeAsync();

			return await Run(ctxt);
		}

		static string TypeName(object? obj) =>
			obj is null ? "null" : $"typeof({ArgumentFormatter.FormatTypeName(obj.GetType())})";
	}
}
