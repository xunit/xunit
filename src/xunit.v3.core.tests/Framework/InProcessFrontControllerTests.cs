using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public static class InProcessFrontControllerTests
{
	static readonly Assembly thisAssembly = typeof(InProcessFrontControllerTests).Assembly;

	public static class Ctor
	{
		[Fact]
		public static void GuardClauses()
		{
			var testFramework = Mocks.TestFramework();

			Assert.Throws<ArgumentNullException>("testFramework", () => new InProcessFrontController(null!, thisAssembly, null));
			Assert.Throws<ArgumentNullException>("testAssembly", () => new InProcessFrontController(testFramework, null!, null));
		}

		[Fact]
		public static void PropertiesReturnValuesFromDiscoverer()
		{
			var frontController = TestableInProcessFrontController.Create();

			Assert.Equal(TestData.DefaultTestFrameworkDisplayName, frontController.TestFrameworkDisplayName);
		}
	}

	public static class Find
	{
		[Fact]
		public static async ValueTask GuardClauses()
		{
			static bool filter(ITestCase testCase) => true;

			var frontController = TestableInProcessFrontController.Create();
			var options = TestData.TestFrameworkDiscoveryOptions();
			using var cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<ArgumentNullException>("options", () => frontController.Find(null, null!, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("filter", () => frontController.Find(null, options, null!, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("cancellationTokenSource", () => frontController.Find(null, options, filter, null!).AsTask());
		}

		[Fact]
		public static async ValueTask SendsStartingAndCompleteMessages()
		{
			static bool filter(ITestCase testCase) => true;

			var frontController = TestableInProcessFrontController.Create(configFilePath: "/path/to/config.json");
			var messageSink = SpyMessageSink.Capture();
			var options = TestData.TestFrameworkDiscoveryOptions();
			using var cts = new CancellationTokenSource();

			await frontController.Find(messageSink, options, filter, cts);

			Assert.Collection(
				messageSink.Messages,
				msg =>
				{
					var starting = Assert.IsType<IDiscoveryStarting>(msg, exactMatch: false);
#if XUNIT_AOT
					Assert.StartsWith("xunit.v3.core.aot.tests", starting.AssemblyName);
					Assert.StartsWith("xunit.v3.core.aot.tests", Path.GetFileName(starting.AssemblyPath));
#elif BUILD_X86 && NETFRAMEWORK
					Assert.StartsWith("xunit.v3.core.netfx.x86.tests", starting.AssemblyName);
					Assert.StartsWith("xunit.v3.core.netfx.x86.tests", Path.GetFileName(starting.AssemblyPath));
#elif BUILD_X86 && NETCOREAPP
					Assert.StartsWith("xunit.v3.core.netcore.x86.tests", starting.AssemblyName);
					Assert.StartsWith("xunit.v3.core.netcore.x86.tests", Path.GetFileName(starting.AssemblyPath));
#elif NETFRAMEWORK
					Assert.StartsWith("xunit.v3.core.netfx.tests", starting.AssemblyName);
					Assert.StartsWith("xunit.v3.core.netfx.tests", Path.GetFileName(starting.AssemblyPath));
#elif NETCOREAPP
					Assert.StartsWith("xunit.v3.core.netcore.tests", starting.AssemblyName);
					Assert.StartsWith("xunit.v3.core.netcore.tests", Path.GetFileName(starting.AssemblyPath));
#else
#error Unknown target build environment
#endif
					Assert.Equal(frontController.TestAssemblyUniqueID, starting.AssemblyUniqueID);
					Assert.Equal("/path/to/config.json", starting.ConfigFilePath);
				},
				msg =>
				{
					var complete = Assert.IsType<IDiscoveryComplete>(msg, exactMatch: false);
					Assert.Equal(frontController.TestAssemblyUniqueID, complete.AssemblyUniqueID);
					Assert.Equal(0, complete.TestCasesToRun);
				}
			);
		}

		[Fact]
		public static async ValueTask ReportsDiscoveredTestCasesAndCountsTestCasesWhichPassFilter()
		{
			var messageSink = SpyMessageSink.Capture();
			var options = TestData.TestFrameworkDiscoveryOptions();
			var validTestCase = Mocks.TestCase(uniqueID: "1");
			var invalidTestCase = Mocks.TestCase(uniqueID: "2");
			bool filter(ITestCase testCase) => testCase == validTestCase;
			using var cts = new CancellationTokenSource();
			var callbackCalls = new List<(ITestCase testCase, bool passedFilter)>();
			var frontController = TestableInProcessFrontController.Create(
				find: async (callback, _, _, _) =>
				{
					await callback(validTestCase);
					await callback(invalidTestCase);
				}
			);

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

		[Fact(DisableParallelization = true)]
		public static async ValueTask RejectsDuplicatedTestCaseUniqueID()
		{
			var diagnosticMessageSink = SpyMessageSink.Capture();
			TestContextInternal.Current.DiagnosticMessageSink = diagnosticMessageSink;

			var messageSink = SpyMessageSink.Capture();
			var options = TestData.TestFrameworkDiscoveryOptions();
			var validTestCase = Mocks.TestCase(uniqueID: "1", testCaseDisplayName: "valid-test-case");
			var invalidTestCase = Mocks.TestCase(uniqueID: "1", testCaseDisplayName: "invalid-test-case");
			bool filter(ITestCase testCase) => true;
			using var cts = new CancellationTokenSource();
			var callbackCalls = new List<(ITestCase testCase, bool passedFilter)>();
			var frontController = TestableInProcessFrontController.Create(
				find: async (callback, _, _, _) =>
				{
					await callback(validTestCase);
					await callback(invalidTestCase);
				}
			);

			await frontController.Find(messageSink, options, filter, cts, discoveryCallback: frontControllerCallback);

			// Make sure we got a diagnostic message indicating the issue
			var diagnosticMessage = Assert.Single(diagnosticMessageSink.Messages.OfType<IDiagnosticMessage>());
			Assert.Equal(
				"Warning: Rejecting v3 test case with duplicate unique ID" + Environment.NewLine +
				"  Original:" + Environment.NewLine +
				"    ID:      1" + Environment.NewLine +
				"    Name:    valid-test-case" + Environment.NewLine +
				"    Method:  test-class-name.test-method" + Environment.NewLine +
				"  Duplicate:" + Environment.NewLine +
				"    ID:      1" + Environment.NewLine +
				"    Name:    invalid-test-case" + Environment.NewLine +
				"    Method:  test-class-name.test-method",
				diagnosticMessage.Message
			);

			ValueTask<bool> frontControllerCallback(
				ITestCase testCase,
				bool passedFilter)
			{
				callbackCalls.Add((testCase, passedFilter));
				return new(true);
			}
		}
	}

	public static class FindAndRun
	{
		[Fact]
		public static async ValueTask GuardClauses()
		{
			static bool filter(ITestCase testCase) => true;

			var frontController = TestableInProcessFrontController.Create();
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			using var cts = new CancellationTokenSource();

			await Assert.ThrowsAsync<ArgumentNullException>("messageSink", () => frontController.FindAndRun(null!, discoveryOptions, executionOptions, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("discoveryOptions", () => frontController.FindAndRun(messageSink, null!, executionOptions, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("executionOptions", () => frontController.FindAndRun(messageSink, discoveryOptions, null!, filter, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("filter", () => frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, null!, cts).AsTask());
			await Assert.ThrowsAsync<ArgumentNullException>("cancellationTokenSource", () => frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, filter, null!).AsTask());
		}

		[Fact]
		public static async ValueTask RunsTestCasesWhichPassFilter()
		{
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			var validTestCase = Mocks.TestCase(uniqueID: "1");
			var invalidTestCase = Mocks.TestCase(uniqueID: "2");
			bool filter(ITestCase testCase) => testCase == validTestCase;
			using var cts = new CancellationTokenSource();
			var executedTestCases = default(IReadOnlyCollection<ITestCase>?);
			var frontController = TestableInProcessFrontController.Create(
				find: async (callback, _, _, _) =>
				{
					await callback(validTestCase);
					await callback(invalidTestCase);
				},
				runTestCases: async (testCases, _, _, _) => executedTestCases = testCases
			);

			await frontController.FindAndRun(messageSink, discoveryOptions, executionOptions, filter, cts);

			Assert.NotNull(executedTestCases);
			var runTestCase = Assert.Single(executedTestCases);
			Assert.Same(validTestCase, runTestCase);
		}

		[Fact]
		public static async ValueTask DisposesOfTestCases()
		{
			var asyncDisposeCalled = false;
			var disposeCalled = false;
			var messageSink = SpyMessageSink.Capture();
			var discoveryOptions = TestData.TestFrameworkDiscoveryOptions();
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			using var cts = new CancellationTokenSource();
			var asyncDisposableTestCase = Mocks.TestCaseAsyncDisposable(() => asyncDisposeCalled = true, uniqueID: "1");
			var disposableTestCase = Mocks.TestCaseDisposable(() => disposeCalled = true, uniqueID: "2");
			var frontController = TestableInProcessFrontController.Create(
				find: async (callback, _, _, _) =>
				{
					await callback(asyncDisposableTestCase);
					await callback(disposableTestCase);
				}
			);

			// Use a false filter to ensure that test cases are disposed even if they weren't run
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
			Func<Func<ITestCase, ValueTask<bool>>, ITestFrameworkDiscoveryOptions, Type[]?, CancellationToken?, ValueTask>? find = null,
			Func<IReadOnlyCollection<ITestCase>, IMessageSink, ITestFrameworkExecutionOptions, CancellationToken?, ValueTask>? runTestCases = null,
			string? configFilePath = null)
		{
			var discoverer = Mocks.TestFrameworkDiscoverer(find: find);
			var executor = Mocks.TestFrameworkExecutor(runTestCases: runTestCases);

			return new TestableInProcessFrontController(discoverer, executor, thisAssembly, configFilePath);
		}
	}
}
