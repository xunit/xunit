using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestAssemblyRunnerTests
{
	public class RunAsync
	{
		// This test is forced to use the aggressive algorithm so that we know we're running in a thread pool with
		// a single thread. The default conserative algorithm runs in the .NET Thread Pool, so our async continuation
		// could end up on any thread, despite the fact that are limited to running one test at a time.
		[Fact]
		public static async ValueTask Parallel_SingleThread_Aggressive()
		{
			var passing = TestData.XunitTestCase<ClassUnderTest>("Passing");
			var other = TestData.XunitTestCase<ClassUnderTest>("Other");
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetMaxParallelThreads(1);
			options.SetParallelAlgorithm(ParallelAlgorithm.Aggressive);
			var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, executionOptions: options);

			await runner.RunAsync();

			var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
			Assert.Equal(threadIDs[0], threadIDs[1]);
		}

		[Fact]
		public static async ValueTask NonParallel()
		{
			var passing = TestData.XunitTestCase<ClassUnderTest>("Passing");
			var other = TestData.XunitTestCase<ClassUnderTest>("Other");
			var options = TestData.TestFrameworkExecutionOptions();
			options.SetDisableParallelization(true);
			var runner = TestableXunitTestAssemblyRunner.Create(testCases: new[] { passing, other }, executionOptions: options);

			await runner.RunAsync();

			var threadIDs = runner.TestCasesRun.Select(x => x.Item1).ToList();
			Assert.Equal(threadIDs[0], threadIDs[1]);
		}
	}

	class ClassUnderTest
	{
		[Fact]
		public void Passing() { Thread.Sleep(0); }

		[Fact]
		public void Other() { Thread.Sleep(0); }
	}

	class TestableXunitTestAssemblyRunner : XunitTestAssemblyRunner
	{
		readonly _IMessageSink executionMessageSink;
		readonly _ITestFrameworkExecutionOptions executionOptions;
		readonly _ITestAssembly testAssembly;
		readonly IReadOnlyCollection<IXunitTestCase> testCases;

		public ConcurrentBag<Tuple<int, IXunitTestCase>> TestCasesRun = new();

		TestableXunitTestAssemblyRunner(
			_ITestAssembly testAssembly,
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			this.testAssembly = testAssembly;
			this.testCases = testCases;
			this.executionMessageSink = executionMessageSink;
			this.executionOptions = executionOptions;
		}

		public static TestableXunitTestAssemblyRunner Create(
			_ITestAssembly? assembly = null,
			IXunitTestCase[]? testCases = null,
			_ITestFrameworkExecutionOptions? executionOptions = null)
		{
			if (testCases is null)
				testCases = new[] { TestData.XunitTestCase<ClassUnderTest>("Passing") };

			return new TestableXunitTestAssemblyRunner(
				assembly ?? testCases.First().TestCollection.TestAssembly,
				testCases,
				SpyMessageSink.Create(),
				executionOptions ?? TestData.TestFrameworkExecutionOptions()
			);
		}

		public async ValueTask<RunSummary> RunAsync()
		{
			await using var ctxt = new XunitTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions);
			await ctxt.InitializeAsync();

			return await RunAsync(ctxt);
		}

		protected override ValueTask<RunSummary> RunTestCollectionAsync(
			XunitTestAssemblyRunnerContext ctxt,
			_ITestCollection testCollection,
			IReadOnlyCollection<IXunitTestCase> testCases)
		{
			foreach (var testCase in testCases)
				TestCasesRun.Add(Tuple.Create(Thread.CurrentThread.ManagedThreadId, testCase));

			Thread.Sleep(5); // Hold onto the worker thread long enough to ensure tests all get spread around
			return new(new RunSummary());
		}
	}
}
