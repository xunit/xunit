using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class AcceptanceTestV3
{
	public ValueTask<List<MessageSinkMessage>> RunAsync(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null) =>
			RunAsync([type], preEnumerateTheories, explicitOption, diagnosticMessageSink);

	public ValueTask<List<MessageSinkMessage>> RunAsync(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
	{
		var tcs = new TaskCompletionSource<List<MessageSinkMessage>>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages: diagnosticMessageSink is not null, internalDiagnosticMessages: diagnosticMessageSink is not null);

				await using var testFramework = new XunitTestFramework();

				var testAssembly = Assembly.GetEntryAssembly()!;
				var discoverer = testFramework.GetDiscoverer(testAssembly);
				var testCases = new List<ITestCase>();
				await discoverer.Find(testCase => { testCases.Add(testCase); return new(true); }, TestData.TestFrameworkDiscoveryOptions(preEnumerateTheories: preEnumerateTheories), types);

				using var runSink = SpyMessageSink<TestAssemblyFinished>.Create();
				var executor = testFramework.GetExecutor(testAssembly);
				await executor.RunTestCases(testCases, runSink, TestData.TestFrameworkExecutionOptions(explicitOption: explicitOption));

				tcs.TrySetResult(runSink.Messages.ToList());
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		});

		return new(tcs.Task);
	}

	public async ValueTask<List<TMessageType>> RunAsync<TMessageType>(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TMessageType : MessageSinkMessage
	{
		var results = await RunAsync(type, preEnumerateTheories, explicitOption, diagnosticMessageSink);
		return results.OfType<TMessageType>().ToList();
	}

	public async ValueTask<List<TMessageType>> RunAsync<TMessageType>(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TMessageType : MessageSinkMessage
	{
		var results = await RunAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink);
		return results.OfType<TMessageType>().ToList();
	}

	public ValueTask<List<ITestResultWithDisplayName>> RunForResultsAsync(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null) =>
			RunForResultsAsync(new[] { type }, preEnumerateTheories, explicitOption, diagnosticMessageSink);

	public async ValueTask<List<ITestResultWithDisplayName>> RunForResultsAsync(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
	{
		var results = await RunAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink);
		return
			results
				.OfType<TestResultMessage>()
				.Select(result => TestResultFactory(result, results.OfType<TestStarting>().Where(ts => ts.TestUniqueID == result.TestUniqueID).Single().TestDisplayName))
				.WhereNotNull()
				.ToList();
	}

	public async ValueTask<List<TResult>> RunForResultsAsync<TResult>(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TResult : ITestResultWithDisplayName
	{
		var results = await RunForResultsAsync(type, preEnumerateTheories, explicitOption, diagnosticMessageSink);
		return results.OfType<TResult>().ToList();
	}

	public async ValueTask<List<TResult>> RunForResultsAsync<TResult>(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TResult : ITestResultWithDisplayName
	{
		var results = await RunForResultsAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink);
		return results.OfType<TResult>().ToList();
	}

	public static ITestResultWithDisplayName? TestResultFactory(
		TestResultMessage result,
		string testDisplayName)
	{
		if (result is TestPassed passed)
			return TestPassedWithDisplayName.FromTestPassed(passed, testDisplayName);
		if (result is TestFailed failed)
			return TestFailedWithDisplayName.FromTestFailed(failed, testDisplayName);
		if (result is TestSkipped skipped)
			return TestSkippedWithDisplayName.FromTestSkipped(skipped, testDisplayName);
		if (result is TestNotRun notRun)
			return TestNotRunWithDisplayName.FromTestNotRun(notRun, testDisplayName);

		return null;
	}
}
