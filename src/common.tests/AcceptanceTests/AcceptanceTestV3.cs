using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class AcceptanceTestV3
{
	public Task<List<_MessageSinkMessage>> RunAsync(Type type) => RunAsync(new[] { type });

	public Task<List<_MessageSinkMessage>> RunAsync(Type[] types)
	{
		var tcs = new TaskCompletionSource<List<_MessageSinkMessage>>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				var diagnosticMessageSink = new _NullMessageSink();
				await using var testFramework = new XunitTestFramework(diagnosticMessageSink, configFileName: null);

				using var discoverySink = SpyMessageSink<_DiscoveryComplete>.Create();
				var assemblyInfo = Reflector.Wrap(Assembly.GetEntryAssembly()!);
				var discoverer = testFramework.GetDiscoverer(assemblyInfo);
				foreach (var type in types)
				{
					discoverer.Find(type.FullName!, discoverySink, _TestFrameworkOptions.ForDiscovery());
					discoverySink.Finished.WaitOne();
					discoverySink.Finished.Reset();
				}

				var testCases = discoverySink.Messages.OfType<_TestCaseDiscovered>().Select(msg => msg.Serialization).ToArray();

				using var runSink = SpyMessageSink<_TestAssemblyFinished>.Create();
				var executor = testFramework.GetExecutor(assemblyInfo);
				executor.RunTests(testCases, runSink, _TestFrameworkOptions.ForExecution());
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
		where TMessageType : _MessageSinkMessage
	{
		var results = await RunAsync(type);
		return results.OfType<TMessageType>().ToList();
	}

	public async Task<List<TMessageType>> RunAsync<TMessageType>(Type[] types)
		where TMessageType : _MessageSinkMessage
	{
		var results = await RunAsync(types);
		return results.OfType<TMessageType>().ToList();
	}
}
