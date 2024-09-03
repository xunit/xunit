using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// This class is used to do discovery and execution of xUnit.net v3 tests.
/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
/// instead of using this class directly.
/// </summary>
public class Xunit3 : IFrontController
{
	static readonly Version Version_0_3_0 = new(0, 3, 0);

	readonly IMessageSink? diagnosticMessageSink;
	readonly XunitProjectAssembly projectAssembly;
	readonly ISourceInformationProvider? sourceInformationProvider;
	readonly ITestProcessLauncher testProcessLauncher;

	Xunit3(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider,
		IMessageSink? diagnosticMessageSink,
		ITestProcessLauncher testProcessLauncher)
	{
		this.projectAssembly = projectAssembly;
		this.sourceInformationProvider = sourceInformationProvider;
		this.diagnosticMessageSink = diagnosticMessageSink;
		this.testProcessLauncher = Guard.ArgumentNotNull(testProcessLauncher);

		using var process = testProcessLauncher.Launch(projectAssembly, ["-assemblyInfo"]) ?? throw new InvalidOperationException("Process was null");
		if (!process.WaitForExit(60_000))
			throw new InvalidOperationException("Test process did not respond within 60 seconds");

		var output = process.StandardOutput.ReadToEnd().Trim(' ', '\r', '\n');
		if (!JsonDeserializer.TryDeserialize(output, out var json))
			throw new InvalidOperationException("Test process terminated unexpectedly." + (output.Length > 0 ? (" Output:" + Environment.NewLine + output) : string.Empty));
		if (json is not Dictionary<string, object?> root)
			throw new InvalidOperationException("Test process did not return valid JSON (non-object). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("core-framework", out var coreFrameworkObject) || coreFrameworkObject is not string coreFrameworkString || !Version.TryParse(coreFrameworkString, out var coreFramework))
			throw new InvalidOperationException("Test process did not return valid JSON ('core-framework' is missing or malformed). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("core-framework-informational", out var coreFrameworkInformationalObject) || coreFrameworkInformationalObject is not string coreFrameworkInformational)
			throw new InvalidOperationException("Test process did not return valid JSON ('core-framework-informational' is missing). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("target-framework", out var targetFrameworkObject) || targetFrameworkObject is not string targetFramework)
			throw new InvalidOperationException("Test process did not return valid JSON ('target-framework' is missing). Output:" + Environment.NewLine + output);
		if (!root.TryGetValue("test-framework", out var testFrameworkObject) || testFrameworkObject is not string testFramework)
			throw new InvalidOperationException("Test process did not return valid JSON ('test-framework' is missing). Output:" + Environment.NewLine + output);

		CoreFrameworkVersion = coreFramework;
		CoreFrameworkVersionInformational = coreFrameworkInformational;
		TargetFramework = targetFramework;
		TestFrameworkDisplayName = testFramework;
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
	}

	/// <inheritdoc/>
	public bool CanUseAppDomains => false;

	/// <summary>
	/// Gets the version of <c>xunit.v3.core.dll</c> the test assembly is linked against.
	/// </summary>
	public Version CoreFrameworkVersion { get; }

	/// <summary>
	/// Gets the informational version of <c>xunit.v3.core.dll</c> the test assembly
	/// is linked against.
	/// </summary>
	public string CoreFrameworkVersionInformational { get; }

	/// <inheritdoc/>
	public string TargetFramework { get; }

	/// <inheritdoc/>
	public string TestAssemblyUniqueID { get; }

	/// <inheritdoc/>
	public string TestFrameworkDisplayName { get; }

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <inheritdoc/>
	public void Find(
		IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var arguments = Xunit3ArgumentFactory.ForFind(
			CoreFrameworkVersion,
			settings.Options,
			settings.Filters,
			projectAssembly.ConfigFileName,
			ListOption.Discovery,
			settings.LaunchOptions.WaitForDebugger
		);

		var synchronousMessageReporting =
			CoreFrameworkVersion >= Version_0_3_0
				&& settings.Options.GetSynchronousMessageReportingOrDefault();

		var process =
			testProcessLauncher.Launch(projectAssembly, arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		var delegatingDiagnosticSink = new DelegatingProcessCancellationSink(process, diagnosticMessageSink);
		var delegatingMessageSink = new DelegatingProcessCancellationSink(process, messageSink);

		// The '-list discovery' only sends test cases, not starting & complete messages,
		// so we'll fabricate them ourselves.
		bool SendDiscoveryStarting(string assemblyUniqueID) =>
			delegatingMessageSink.OnMessage(new DiscoveryStarting
			{
				AssemblyName = projectAssembly.AssemblyDisplayName,
				AssemblyPath = projectAssembly.AssemblyFileName,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = projectAssembly.ConfigFileName,
			});

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			var assemblyUniqueID = default(string);
			var testCaseCount = 0;
			var collectSourceInformation = settings.Options.GetIncludeSourceInformationOrDefault();

			try
			{
				while (true)
				{
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					try
					{
						var message = MessageSinkMessageDeserializer.Deserialize(line, delegatingDiagnosticSink);

						if (message is null)
							delegatingDiagnosticSink.OnMessage(new InternalDiagnosticMessage("Received unparseable output from test process: " + line));
						else if (message is IDiagnosticMessage or IInternalDiagnosticMessage)
							delegatingDiagnosticSink.OnMessage(message);
						else if (message is ITestCaseDiscovered testDiscovered)
						{
							// Don't overwrite the source information if it came directly from the test framework
							if (collectSourceInformation && sourceInformationProvider is not null && testDiscovered.SourceFilePath is null && testDiscovered.SourceLineNumber is null)
							{
								var sourceInformation = sourceInformationProvider.GetSourceInformation(testDiscovered.TestClassName, testDiscovered.TestMethodName);
								testDiscovered = testDiscovered.WithSourceInfo(sourceInformation.SourceFile, sourceInformation.SourceLine);
							}

							if (assemblyUniqueID is null)
							{
								assemblyUniqueID = testDiscovered.AssemblyUniqueID;
								SendDiscoveryStarting(assemblyUniqueID);
							}

							++testCaseCount;
							delegatingMessageSink.OnMessage(testDiscovered);
						}
					}
					finally
					{
						if (synchronousMessageReporting)
							process.StandardInput.WriteLine();
					}
				}
			}
			finally
			{
				// Dispose first so we don't race against anything waiting for discovery complete.
				// We want the process to be fully cleaned up before runners move on.
				process.Dispose();

				// If we didn't see any test cases, we can compute a unique ID. We also need to make
				// sure we send the starting message before the complete message in this case.
				if (assemblyUniqueID is null)
				{
					assemblyUniqueID = UniqueIDGenerator.ForAssembly(projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
					SendDiscoveryStarting(assemblyUniqueID);
				}

				messageSink.OnMessage(new DiscoveryComplete
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestCasesToRun = testCaseCount,
				});
			}
		});
	}

	/// <inheritdoc/>
	public void FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var arguments = Xunit3ArgumentFactory.ForFindAndRun(
			CoreFrameworkVersion,
			settings.DiscoveryOptions,
			settings.ExecutionOptions,
			settings.Filters,
			projectAssembly.ConfigFileName,
			settings.LaunchOptions.WaitForDebugger
		);

		var synchronousMessageReporting =
			CoreFrameworkVersion >= Version_0_3_0
				&& (settings.ExecutionOptions.GetSynchronousMessageReporting() ?? settings.DiscoveryOptions.GetSynchronousMessageReporting() ?? false);

		RunInternal(messageSink, arguments, synchronousMessageReporting);
	}

	/// <inheritdoc/>
	public void Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);
		Guard.ArgumentNotNullOrEmpty(settings.SerializedTestCases);

		var arguments = Xunit3ArgumentFactory.ForRun(
			CoreFrameworkVersion,
			settings.Options,
			settings.SerializedTestCases,
			projectAssembly.ConfigFileName,
			settings.LaunchOptions.WaitForDebugger
		);

		var synchronousMessageReporting =
			CoreFrameworkVersion >= Version_0_3_0
				&& settings.Options.GetSynchronousMessageReportingOrDefault();

		RunInternal(messageSink, arguments, synchronousMessageReporting);
	}

	void RunInternal(
		IMessageSink messageSink,
		List<string> arguments,
		bool synchronousMessageReporting)
	{
		var process =
			testProcessLauncher.Launch(projectAssembly, arguments)
				?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Could not launch test process. Test assembly '{0}', arguments: '{1}'", projectAssembly.AssemblyFileName, string.Join(" ", arguments)));

		var delegatingDiagnosticSink = new DelegatingProcessCancellationSink(process, diagnosticMessageSink);
		var delegatingMessageSink = new DelegatingProcessCancellationSink(process, messageSink);

		ThreadPool.QueueUserWorkItem(async _ =>
		{
			try
			{
				while (true)
				{
					var line = await process.StandardOutput.ReadLineAsync();
					if (line is null)
						break;

					try
					{
						var message = MessageSinkMessageDeserializer.Deserialize(line, delegatingDiagnosticSink);

						if (message is null)
							delegatingDiagnosticSink.OnMessage(new InternalDiagnosticMessage("Received unparseable output from test process: " + line));
						else if (message is IDiagnosticMessage or IInternalDiagnosticMessage)
							delegatingDiagnosticSink.OnMessage(message);
						else
							delegatingMessageSink.OnMessage(message);

						if (message is ITestAssemblyFinished)
							break;
					}
					finally
					{
						if (synchronousMessageReporting)
							process.StandardInput.WriteLine();
					}
				}
			}
			finally
			{
				process.Dispose();
			}
		});
	}

	// Factory method

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used
	/// for both discovery and execution of xUnit.net v3 tests.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="testProcessLauncher">The test process launcher to use for starting the xUnit.net v3 test
	/// process. If not specified, will use <see cref="LocalOutOfProcessTestProcessLauncher"/>.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider = null,
		IMessageSink? diagnosticMessageSink = null,
		ITestProcessLauncher? testProcessLauncher = null)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.FileExists(projectAssembly.AssemblyFileName);

		return new Xunit3(projectAssembly, sourceInformationProvider, diagnosticMessageSink, testProcessLauncher ?? LocalOutOfProcessTestProcessLauncher.Instance);
	}
}
