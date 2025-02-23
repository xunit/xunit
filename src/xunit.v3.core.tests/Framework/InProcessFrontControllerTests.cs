using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class InProcessFrontControllerTests
{
	static readonly Assembly thisAssembly = typeof(InProcessFrontControllerTests).Assembly;

	public class Ctor
	{
		[Fact]
		public void GuardClauses()
		{
			var testFramework = Mocks.TestFramework();

			Assert.Throws<ArgumentNullException>("testFramework", () => new InProcessFrontController(null!, thisAssembly, null));
			Assert.Throws<ArgumentNullException>("testAssembly", () => new InProcessFrontController(testFramework, null!, null));
		}

		[Fact]
		public void PropertiesReturnValuesFromDiscoverer()
		{
			var frontController = TestableInProcessFrontController.Create();

			Assert.Equal(TestData.DefaultTestFrameworkDisplayName, frontController.TestFrameworkDisplayName);
		}
	}

	public class Find
	{
		[Fact]
		public async ValueTask GuardClauses()
		{
			var frontController = TestableInProcessFrontController.Create();
			var options = TestData.TestFrameworkDiscoveryOptions();
			var filter = (ITestCase testCase) => true;
			using var cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<ArgumentNullException>("options", () => frontController.Find(null, null!, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("filter", () => frontController.Find(null, options, null!, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("cancellationTokenSource", () => frontController.Find(null, options, filter, null!).AsTask());
		}

		[Fact]
		public async ValueTask SendsStartingAndCompleteMessages()
		{
			var frontController = TestableInProcessFrontController.Create(configFilePath: "/path/to/config.json");
			var messageSink = SpyMessageSink.Capture();
			var options = TestData.TestFrameworkDiscoveryOptions();
			var filter = (ITestCase testCase) => true;
			using var cts = new CancellationTokenSource();

			await frontController.Find(messageSink, options, filter, cts);

			Assert.Collection(
				messageSink.Messages,
				msg =>
				{
					var starting = Assert.IsAssignableFrom<IDiscoveryStarting>(msg);
#if BUILD_X86
					Assert.StartsWith("xunit.v3.core.x86.tests", starting.AssemblyName);
					Assert.Equal("xunit.v3.core.x86.tests", Path.GetFileNameWithoutExtension(starting.AssemblyPath));
#else
					Assert.StartsWith("xunit.v3.core.tests", starting.AssemblyName);
					Assert.Equal("xunit.v3.core.tests", Path.GetFileNameWithoutExtension(starting.AssemblyPath));
#endif
					Assert.Equal(frontController.TestAssemblyUniqueID, starting.AssemblyUniqueID);
					Assert.Equal("/path/to/config.json", starting.ConfigFilePath);
				},
				msg =>
				{
					var complete = Assert.IsAssignableFrom<IDiscoveryComplete>(msg);
					Assert.Equal(frontController.TestAssemblyUniqueID, complete.AssemblyUniqueID);
					Assert.Equal(0, complete.TestCasesToRun);
				}
			);
		}

		[Fact]
		public async ValueTask ReportsDiscoveredTestCasesAndCountsTestCasesWhichPassFilter()
		{
			var frontController = TestableInProcessFrontController.Create();
			var messageSink = SpyMessageSink.Capture();
			var options = TestData.TestFrameworkDiscoveryOptions();
			var validTestCase = Mocks.XunitTestCase<Find>(nameof(ReportsDiscoveredTestCasesAndCountsTestCasesWhichPassFilter));
			var invalidTestCase = Mocks.XunitTestCase<Find>(nameof(SendsStartingAndCompleteMessages));
			var filter = (ITestCase testCase) => testCase == validTestCase;
			using var cts = new CancellationTokenSource();
			var callbackCalls = new List<(ITestCase testCase, bool passedFilter)>();
			frontController
				.Discoverer
				.WhenForAnyArgs(d => d.Find(null!, null!))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<Func<ITestCase, ValueTask<bool>>>();
#pragma warning disable xUnit1031  // Test methods must not use blocking task operations
					callback(validTestCase).GetAwaiter().GetResult();
					callback(invalidTestCase).GetAwaiter().GetResult();
#pragma warning restore xUnit1031  // Test methods must not use blocking task operations
				});
			ValueTask<bool> frontControllerCallback(
				ITestCase testCase,
				bool passedFilter)
			{
				callbackCalls.Add((testCase, passedFilter));
				return new(true);
			}

			await frontController.Find(messageSink, options, filter, cts, discoveryCallback: frontControllerCallback);

			var complete = Assert.Single(messageSink.Messages.OfType<IDiscoveryComplete>());
			Assert.Equal(1, complete.TestCasesToRun);
			Assert.Collection(
				callbackCalls,
				callback =>
				{
					Assert.Same(validTestCase, callback.testCase);
					Assert.True(callback.passedFilter);
				},
				callback =>
				{
					Assert.Same(invalidTestCase, callback.testCase);
					Assert.False(callback.passedFilter);
				}
			);
		}
	}

	public class FindAndRun
	{
		[Fact]
		public async ValueTask GuardClauses()
		{
			var frontController = TestableInProcessFrontController.Create();
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			var filter = (ITestCase testCase) => true;
			using var cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<ArgumentNullException>("messageSink", () => frontController.FindAndRun(null!, discoveryOptions, executionOptions, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("discoveryOptions", () => frontController.FindAndRun(messageSink, null!, executionOptions, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("executionOptions", () => frontController.FindAndRun(messageSink, discoveryOptions, null!, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("filter", () => frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, null!, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("cancellationTokenSource", () => frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, filter, null!).AsTask());
		}

		[Fact]
		public async ValueTask RunsTestCasesWhichPassFilter()
		{
			var frontController = TestableInProcessFrontController.Create();
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			var validTestCase = Mocks.XunitTestCase<FindAndRun>(nameof(RunsTestCasesWhichPassFilter));
			var invalidTestCase = Mocks.XunitTestCase<Find>(nameof(GuardClauses));
			var filter = (ITestCase testCase) => testCase == validTestCase;
			using var cts = new CancellationTokenSource();
			var executedTestCases = default(IReadOnlyCollection<ITestCase>?);
			frontController
				.Discoverer
				.WhenForAnyArgs(d => d.Find(null!, null!))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<Func<ITestCase, ValueTask<bool>>>();
#pragma warning disable xUnit1031  // Test methods must not use blocking task operations
					callback(validTestCase).GetAwaiter().GetResult();
					callback(invalidTestCase).GetAwaiter().GetResult();
#pragma warning restore xUnit1031  // Test methods must not use blocking task operations
				});
			frontController
				.Executor
				.WhenForAnyArgs(e => e.RunTestCases(null!, null!, null!))
				.Do(callInfo => executedTestCases = callInfo.Arg<IReadOnlyCollection<ITestCase>>());

			await frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, filter, cts);

			Assert.NotNull(executedTestCases);
			var runTestCase = Assert.Single(executedTestCases);
			Assert.Same(validTestCase, runTestCase);
		}

		[Fact]
		public async ValueTask DisposesOfTestCases()
		{
			var asyncDisposeCalled = false;
			var disposeCalled = false;
			var frontController = TestableInProcessFrontController.Create();
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			using var cts = new CancellationTokenSource();
			var asyncDisposableTestCase = Mocks.XunitTestCase<FindAndRun>(nameof(RunsTestCasesWhichPassFilter), asyncDisposeCallback: () => asyncDisposeCalled = true);
			var disposableTestCase = Mocks.XunitTestCase<Find>(nameof(GuardClauses), disposeCallback: () => disposeCalled = true);
			frontController
				.Discoverer
				.WhenForAnyArgs(d => d.Find(null!, null!))
				.Do(callInfo =>
				{
					var callback = callInfo.Arg<Func<ITestCase, ValueTask<bool>>>();
#pragma warning disable xUnit1031  // Test methods must not use blocking task operations
					callback(asyncDisposableTestCase).GetAwaiter().GetResult();
					callback(disposableTestCase).GetAwaiter().GetResult();
#pragma warning restore xUnit1031  // Test methods must not use blocking task operations
				});
			frontController
				.Executor
				.WhenForAnyArgs(e => e.RunTestCases(null!, null!, null!))
				.Do(callInfo => { });

			// Use a false filter to ensure that test cases to disposed even if they weren't run
			await frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, filter: testCase => false, cts);

			Assert.True(asyncDisposeCalled);
			Assert.True(disposeCalled);
		}
	}

	class TestableInProcessFrontController : InProcessFrontController
	{
		public readonly ITestFrameworkDiscoverer Discoverer;
		public readonly ITestFrameworkExecutor Executor;
		public readonly Assembly TestAssembly;

		TestableInProcessFrontController(
			ITestFrameworkDiscoverer discoverer,
			ITestFrameworkExecutor executor,
			Assembly testAssembly,
			string? configFilePath) :
				base(Mocks.TestFramework(discoverer, executor), testAssembly, configFilePath)
		{
			Discoverer = discoverer;
			Executor = executor;
			TestAssembly = testAssembly;
		}

		public static TestableInProcessFrontController Create(
			ITestFrameworkDiscoverer? discoverer = null,
			ITestFrameworkExecutor? executor = null,
			Assembly? testAssembly = null,
			string? configFilePath = null)
		{
			discoverer ??= Mocks.TestFrameworkDiscoverer();
			executor ??= Mocks.TestFrameworkExecutor();
			testAssembly ??= thisAssembly;

			return new TestableInProcessFrontController(discoverer, executor, testAssembly, configFilePath);
		}
	}
}
