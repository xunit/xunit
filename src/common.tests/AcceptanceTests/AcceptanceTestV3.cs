using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class AcceptanceTestV3
{
	public ValueTask<List<_MessageSinkMessage>> RunAsync(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes) =>
			RunAsync(new[] { type }, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);

	public ValueTask<List<_MessageSinkMessage>> RunAsync(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
	{
		var tcs = new TaskCompletionSource<List<_MessageSinkMessage>>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages: true, internalDiagnosticMessages: false);

				await using var testFramework = new XunitTestFramework();

				var assemblyInfo = Reflector.Wrap(Assembly.GetEntryAssembly()!, additionalAssemblyAttributes);
				var discoverer = testFramework.GetDiscoverer(assemblyInfo);
				var testCases = new List<_ITestCase>();
				await discoverer.Find(testCase => { testCases.Add(testCase); return new(true); }, _TestFrameworkOptions.ForDiscovery(preEnumerateTheories: preEnumerateTheories), types);

				using var runSink = SpyMessageSink<_TestAssemblyFinished>.Create();
				var executor = testFramework.GetExecutor(assemblyInfo);
				await executor.RunTestCases(testCases, runSink, _TestFrameworkOptions.ForExecution(explicitOption: explicitOption));

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
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
			where TMessageType : _MessageSinkMessage
	{
		var results = await RunAsync(type, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);
		return results.OfType<TMessageType>().ToList();
	}

	public async ValueTask<List<TMessageType>> RunAsync<TMessageType>(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
			where TMessageType : _MessageSinkMessage
	{
		var results = await RunAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);
		return results.OfType<TMessageType>().ToList();
	}

	public ValueTask<List<ITestResultWithDisplayName>> RunForResultsAsync(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes) =>
			RunForResultsAsync(new[] { type }, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);

	public async ValueTask<List<ITestResultWithDisplayName>> RunForResultsAsync(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
	{
		var results = await RunAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);
		return
			results
				.OfType<_TestResultMessage>()
				.Select(result => TestResultFactory(result, results.OfType<_TestStarting>().Where(ts => ts.TestUniqueID == result.TestUniqueID).Single().TestDisplayName))
				.WhereNotNull()
				.ToList();
	}

	public async ValueTask<List<TResult>> RunForResultsAsync<TResult>(
		Type type,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
			where TResult : ITestResultWithDisplayName
	{
		var results = await RunForResultsAsync(type, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);
		return results.OfType<TResult>().ToList();
	}

	public async ValueTask<List<TResult>> RunForResultsAsync<TResult>(
		Type[] types,
		bool preEnumerateTheories = true,
		ExplicitOption? explicitOption = null,
		_IMessageSink? diagnosticMessageSink = null,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes)
			where TResult : ITestResultWithDisplayName
	{
		var results = await RunForResultsAsync(types, preEnumerateTheories, explicitOption, diagnosticMessageSink, additionalAssemblyAttributes);
		return results.OfType<TResult>().ToList();
	}

	public static ITestResultWithDisplayName? TestResultFactory(
		_TestResultMessage result,
		string testDisplayName)
	{
		if (result is _TestPassed passed)
			return new TestPassedWithDisplayName(passed, testDisplayName);
		if (result is _TestFailed failed)
			return new TestFailedWithDisplayName(failed, testDisplayName);
		if (result is _TestSkipped skipped)
			return new TestSkippedWithDisplayName(skipped, testDisplayName);
		if (result is _TestNotRun notRun)
			return new TestNotRunWithDisplayName(notRun, testDisplayName);

		return null;
	}
}
