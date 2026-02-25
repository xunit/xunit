using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of test framework interfaces
public static partial class Mocks
{
	public static ITestFramework TestFramework(
		ITestFrameworkDiscoverer? discoverer = null,
		ITestFrameworkExecutor? executor = null,
		string testFrameworkDisplayName = TestData.DefaultTestFrameworkDisplayName) =>
			new MockTestFramework(discoverer ?? TestFrameworkDiscoverer(), executor ?? TestFrameworkExecutor(), testFrameworkDisplayName);

	class MockTestFramework(
		ITestFrameworkDiscoverer discoverer,
		ITestFrameworkExecutor executor,
		string testFrameworkDisplayName) :
			ITestFramework
	{
		public string TestFrameworkDisplayName =>
			testFrameworkDisplayName;

		public ITestFrameworkDiscoverer GetDiscoverer(Assembly assembly) =>
			discoverer;

		public ITestFrameworkExecutor GetExecutor(Assembly assembly) =>
			executor;

		public void SetTestPipelineStartup(ITestPipelineStartup pipelineStartup)
		{ }
	}

	public static ITestFrameworkDiscoverer TestFrameworkDiscoverer(
		ITestAssembly? testAssembly = null,
		Func<Func<ITestCase, ValueTask<bool>>, ITestFrameworkDiscoveryOptions, Type[]?, CancellationToken?, ValueTask>? find = null) =>
			new MockTestFrameworkDiscoverer(testAssembly ?? TestAssembly(), find);

	class MockTestFrameworkDiscoverer(
		ITestAssembly testAssembly,
		Func<Func<ITestCase, ValueTask<bool>>, ITestFrameworkDiscoveryOptions, Type[]?, CancellationToken?, ValueTask>? find) :
			ITestFrameworkDiscoverer
	{
		public ITestAssembly TestAssembly =>
			testAssembly;

		public async ValueTask Find(
			Func<ITestCase, ValueTask<bool>> callback,
			ITestFrameworkDiscoveryOptions discoveryOptions,
			Type[]? types = null,
			CancellationToken? cancellationToken = null)
		{
			if (find is not null)
				await find(callback, discoveryOptions, types, cancellationToken);
		}
	}

	public static ITestFrameworkExecutor TestFrameworkExecutor(Func<IReadOnlyCollection<ITestCase>, IMessageSink, ITestFrameworkExecutionOptions, CancellationToken?, ValueTask>? runTestCases = null) =>
		new MockTestFrameworkExecutor(runTestCases);

	class MockTestFrameworkExecutor(Func<IReadOnlyCollection<ITestCase>, IMessageSink, ITestFrameworkExecutionOptions, CancellationToken?, ValueTask>? runTestCases) :
		ITestFrameworkExecutor
	{
		public async ValueTask RunTestCases(
			IReadOnlyCollection<ITestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions,
			CancellationToken? cancellationToken = null)
		{
			if (runTestCases is not null)
				await runTestCases(testCases, executionMessageSink, executionOptions, cancellationToken);
		}
	}
}
