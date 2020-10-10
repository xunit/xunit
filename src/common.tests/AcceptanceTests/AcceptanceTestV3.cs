using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Sdk;

public class AcceptanceTestV3
{
	public Task<List<IMessageSinkMessage>> RunAsync(Type type) => RunAsync(new[] { type });

	public Task<List<IMessageSinkMessage>> RunAsync(Type[] types)
	{
		var tcs = new TaskCompletionSource<List<IMessageSinkMessage>>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				var diagnosticMessageSink = new NullMessageSink();
				await using var testFramework = new XunitTestFramework(diagnosticMessageSink, configFileName: null);

				using var discoverySink = new SpyMessageSink<IDiscoveryCompleteMessage>();
				var assemblyInfo = Reflector.Wrap(Assembly.GetEntryAssembly()!);
				var discoverer = testFramework.GetDiscoverer(assemblyInfo);
				foreach (var type in types)
				{
					discoverer.Find(type.FullName, includeSourceInformation: false, discoverySink, TestFrameworkOptions.ForDiscovery());
					discoverySink.Finished.WaitOne();
					discoverySink.Finished.Reset();
				}

				var testCases = discoverySink.Messages.OfType<ITestCaseDiscoveryMessage>().Select(msg => msg.TestCase).ToArray();

				using var runSink = new SpyMessageSink<ITestAssemblyFinished>();
				var executor = testFramework.GetExecutor(assemblyInfo);
				executor.RunTests(testCases, runSink, TestFrameworkOptions.ForExecution());
				runSink.Finished.WaitOne();

				tcs.TrySetResult(runSink.Messages.ToList());
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		});

		return tcs.Task;
	}

	public async Task<List<TMessageType>> RunAsync<TMessageType>(Type type)
		where TMessageType : IMessageSinkMessage
	{
		var results = await RunAsync(type);
		return results.OfType<TMessageType>().ToList();
	}

	public async Task<List<TMessageType>> RunAsync<TMessageType>(Type[] types)
		where TMessageType : IMessageSinkMessage
	{
		var results = await RunAsync(types);
		return results.OfType<TMessageType>().ToList();
	}
}
