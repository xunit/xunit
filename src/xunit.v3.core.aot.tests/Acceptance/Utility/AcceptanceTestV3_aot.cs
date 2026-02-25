using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

partial class AcceptanceTestV3
{
	static readonly string acceptanceAssemblyPath;
	static readonly AssemblyMetadata? acceptanceAssemblyMetadata;

	static AcceptanceTestV3()
	{
		// Walk up from the current folder until you find your own bin folder, since sometimes
		// we'll be in "bin\<configuration>\<targetFramework>" and sometimes we'll be in
		// "bin\<configuration>\<targetFramework>\<runtime>\publish". Either way, we are content
		// to use the unpublished version of xunit.v3.acceptance.aot.tests (and in fact, we will
		// never publish this in the normal course of the build script).

		var ourBinFolder = AppContext.BaseDirectory;

		while (ourBinFolder is not null && Path.GetFileName(ourBinFolder) != "bin")
			ourBinFolder = Path.GetDirectoryName(ourBinFolder);

		if (ourBinFolder is null)
			throw new InvalidOperationException($"Could not find our 'bin' folder in '{AppContext.BaseDirectory}'");

		acceptanceAssemblyPath = Path.GetFullPath(Path.Combine(
			ourBinFolder,
			"..", "..",
			"xunit.v3.acceptance.aot.tests", "bin",
#if DEBUG
			"Debug",
#else
			"Release",
#endif
			"net9.0", "xunit.v3.acceptance.aot.tests.dll"
		));

		if (!File.Exists(acceptanceAssemblyPath))
			return;

		acceptanceAssemblyMetadata = new AssemblyMetadata(xunitVersion: 3, targetFramework: ".NETCoreApp,Version=v9.0");
	}

	public static ValueTask<List<IMessageSinkMessage>> RunAsync(
		string type,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null) =>
			RunAsync([type], explicitOption, diagnosticMessageSink);

	public static ValueTask<List<IMessageSinkMessage>> RunAsync(
		string[] types,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
	{
		var tcs = new TaskCompletionSource<List<IMessageSinkMessage>>();

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				if (acceptanceAssemblyMetadata is null)
					throw new TestPipelineException($"Unable to locate acceptance test assembly; expected path: '{acceptanceAssemblyPath}'");

				var projectAssembly = new XunitProjectAssembly(new XunitProject(), acceptanceAssemblyPath, acceptanceAssemblyMetadata);
				projectAssembly.Configuration.ExplicitOption = explicitOption;
				projectAssembly.Configuration.PreEnumerateTheories = true;

				var filters = new XunitFilters();
				foreach (var type in types)
					filters.AddIncludedClassFilter(type);

				await using var xunit3 = Xunit3.ForDiscoveryAndExecution(projectAssembly, diagnosticMessageSink: diagnosticMessageSink);
				var discoveryOptions = TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
				var executionOptions = TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
				var findAndRunSettings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, filters);
				var executionSink = SpyMessageSink.Capture();
				xunit3.FindAndRun(executionSink, findAndRunSettings);

				tcs.TrySetResult(executionSink.Messages);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		});

		return new(tcs.Task);
	}

	public async static ValueTask<List<TMessageType>> RunAsync<TMessageType>(
		string type,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TMessageType : IMessageSinkMessage
	{
		var results = await RunAsync(type, explicitOption, diagnosticMessageSink);
		return results.OfType<TMessageType>().ToList();
	}

	public async static ValueTask<List<TMessageType>> RunAsync<TMessageType>(
		string[] types,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
			where TMessageType : IMessageSinkMessage
	{
		var results = await RunAsync(types, explicitOption, diagnosticMessageSink);
		return results.OfType<TMessageType>().ToList();
	}

	// TODO: Move as many tests as possible to RunForResultsAsync, and then cache the results (unless
	// explicit is On/Only or there is a diagnostic message sink).

	public static ValueTask<List<ITestResultWithMetadata>> RunForResultsAsync(
		string type,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null) =>
			RunForResultsAsync([type], explicitOption, diagnosticMessageSink);

	public async static ValueTask<List<ITestResultWithMetadata>> RunForResultsAsync(
		string[] types,
		ExplicitOption? explicitOption = null,
		IMessageSink? diagnosticMessageSink = null)
	{
		var messages = await RunAsync(types, explicitOption, diagnosticMessageSink);
		return
			messages
				.OfType<ITestResultMessage>()
				.Select(result => TestResultFactory(result, messages))
				.WhereNotNull()
				.ToList();
	}
}
