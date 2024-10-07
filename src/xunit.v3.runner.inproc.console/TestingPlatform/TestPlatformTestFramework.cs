#pragma warning disable CA1033  // Interface hiding is used explicitly here so that tests call the right methods

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.Services;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;
using ITestPlatformMessageBus = Microsoft.Testing.Platform.Messages.IMessageBus;
using ITestPlatformTestFramework = Microsoft.Testing.Platform.Extensions.TestFramework.ITestFramework;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// Implementation of <see cref="ITestPlatformTestFramework"/> to run xUnit.net v3 test projects.
/// </summary>
[ExcludeFromCodeCoverage]
public class TestPlatformTestFramework :
	ExtensionBase, ITestPlatformTestFramework, IDataProducer
{
	readonly IMessageSink? diagnosticMessageSink;
	readonly IMessageSink innerSink;
	readonly IOutputDevice outputDevice;
	readonly XunitProjectAssembly projectAssembly;
	readonly ConcurrentDictionary<SessionUid, CountdownEvent> operationCounterBySessionUid = new();
	readonly IRunnerLogger runnerLogger;
	readonly Assembly testAssembly;
	readonly XunitTrxCapability trxCapability;

	/// <inheritdoc/>
	protected TestPlatformTestFramework(
		IRunnerLogger runnerLogger,
		IMessageSink innerSink,
		IMessageSink? diagnosticMessageSink,
		XunitProjectAssembly projectAssembly,
		Assembly testAssembly,
		XunitTrxCapability trxCapability,
		IOutputDevice outputDevice) :
			base("test framework", "30ea7c6e-dd24-4152-a360-1387158cd41d")
	{
		this.runnerLogger = runnerLogger;
		this.innerSink = innerSink;
		this.diagnosticMessageSink = diagnosticMessageSink;
		this.projectAssembly = projectAssembly;
		this.testAssembly = testAssembly;
		this.trxCapability = trxCapability;
		this.outputDevice = outputDevice;

		SerializationHelper.Instance.AddRegisteredSerializers(testAssembly);
	}

	/// <inheritdoc/>
	public Type[] DataTypesProduced =>
		[typeof(SessionFileArtifact)];

	/// <inheritdoc/>
	public CloseTestSessionResult CloseTestSession(SessionUid sessionUid)
	{
		if (!operationCounterBySessionUid.TryRemove(sessionUid, out var operationCounter))
			return new CloseTestSessionResult { IsSuccess = false, ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Attempted to close unknown session UID '{0}'", sessionUid.Value) };

		operationCounter.Signal();
		operationCounter.Wait();
		operationCounter.Dispose();

		return new CloseTestSessionResult { IsSuccess = true };
	}

	Task<CloseTestSessionResult> ITestPlatformTestFramework.CloseTestSessionAsync(CloseTestSessionContext context) =>
		Task.FromResult(CloseTestSession(Guard.ArgumentNotNull(context).SessionUid));

	/// <inheritdoc/>
	public CreateTestSessionResult CreateTestSession(SessionUid sessionUid)
	{
		if (!operationCounterBySessionUid.TryAdd(sessionUid, new CountdownEvent(1)))
			return new CreateTestSessionResult { IsSuccess = false, ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Attempted to reuse session UID '{0}' already in progress", sessionUid.Value) };

		if (!projectAssembly.Project.Configuration.NoLogoOrDefault)
			runnerLogger.LogImportantMessage(ProjectAssemblyRunner.Banner);

		return new CreateTestSessionResult { IsSuccess = true };
	}

	Task<CreateTestSessionResult> ITestPlatformTestFramework.CreateTestSessionAsync(CreateTestSessionContext context)
	{
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("XUNIT_TESTINGPLATFORM_DEBUG")))
			Debugger.Launch();

		return Task.FromResult(CreateTestSession(Guard.ArgumentNotNull(context).SessionUid));
	}

	async Task ITestPlatformTestFramework.ExecuteRequestAsync(ExecuteRequestContext context)
	{
		Guard.ArgumentNotNull(context);

		if (context.Request is DiscoverTestExecutionRequest discoverRequest)
			await OnDiscover(discoverRequest.Session.SessionUid, context.MessageBus, context.Complete, context.CancellationToken);
		else if (context.Request is RunTestExecutionRequest executionRequest)
			await OnExecute(executionRequest.Session.SessionUid, executionRequest.Filter, context.MessageBus, context.Complete, context.CancellationToken);
	}

	/// <inheritdoc/>
	public ValueTask OnDiscover(
		SessionUid sessionUid,
		ITestPlatformMessageBus messageBus,
		Action operationComplete,
		CancellationToken cancellationToken) =>
			OnRequest(sessionUid, operationComplete, async (projectRunner, pipelineStartup) =>
			{
				// Default to true for Testing Platform
				// TODO: We'd prefer true for Test Explorer and false for `dotnet test`
				projectAssembly.Configuration.PreEnumerateTheories ??= true;

				var messageHandler = new TestPlatformDiscoveryMessageSink(innerSink, projectAssembly.Assembly!.FullName!, sessionUid, messageBus, cancellationToken);
				await projectRunner.Discover(projectAssembly, pipelineStartup, messageHandler);
			}, cancellationToken);

	/// <inheritdoc/>
	public ValueTask OnExecute(
		SessionUid sessionUid,
		ITestExecutionFilter? filter,
		ITestPlatformMessageBus messageBus,
		Action operationComplete,
		CancellationToken cancellationToken) =>
			OnRequest(sessionUid, operationComplete, async (projectRunner, pipelineStartup) =>
			{
				var testCaseIDsToRun = filter switch
				{
					TestNodeUidListFilter filter => filter.TestNodeUids.Select(u => u.Value).ToHashSet(StringComparer.OrdinalIgnoreCase),
					_ => null,
				};

				// Default to true for Testing Platform
				// TODO: We'd prefer true for Test Explorer and false for `dotnet test`
				projectAssembly.Configuration.PreEnumerateTheories ??= true;

				// If the user wants live output, we'll turn it off in the configuration (so the default reporter doesn't
				// report it) and instead tell the message sink to display it.
				var showLiveOutput = projectAssembly.Configuration.ShowLiveOutputOrDefault;
				projectAssembly.Configuration.ShowLiveOutput = false;

				var messageHandler = new TestPlatformExecutionMessageSink(innerSink, sessionUid, messageBus, trxCapability, outputDevice, showLiveOutput, cancellationToken);
				await projectRunner.Run(projectAssembly, messageHandler, diagnosticMessageSink, runnerLogger, pipelineStartup, testCaseIDsToRun);

				foreach (var output in projectAssembly.Project.Configuration.Output)
					await messageBus.PublishAsync(this, new SessionFileArtifact(sessionUid, new FileInfo(output.Value), Path.GetFileNameWithoutExtension(output.Value)));
			}, cancellationToken);

	async ValueTask OnRequest(
		SessionUid sessionUid,
		Action operationComplete,
		Func<ProjectAssemblyRunner, ITestPipelineStartup?, ValueTask> callback,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(operationComplete);
		Guard.ArgumentNotNull(callback);

		if (!operationCounterBySessionUid.TryGetValue(sessionUid, out var operationCounter))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Attempted to execute request against unknown session UID '{0}'", sessionUid.Value), nameof(sessionUid));

		operationCounter.AddCount();

		try
		{
			var pipelineStartup = await ProjectAssemblyRunner.InvokePipelineStartup(testAssembly, diagnosticMessageSink);

			try
			{
				var projectRunner = new ProjectAssemblyRunner(testAssembly, () => cancellationToken.IsCancellationRequested, automatedMode: AutomatedMode.Off);
				await callback(projectRunner, pipelineStartup);
				operationComplete();
			}
			finally
			{
				if (pipelineStartup is not null)
					await pipelineStartup.StopAsync();
			}
		}
		finally
		{
			operationCounter.Signal();
		}
	}

	/// <summary>
	/// Runs the test project.
	/// </summary>
	/// <param name="args">The command line arguments that were passed to the executable</param>
	/// <param name="extensionRegistration">The extension registration callback</param>
	/// <returns>The return code to be returned from Main</returns>
	public static async Task<int> RunAsync(
		string[] args,
		Action<ITestApplicationBuilder, string[]> extensionRegistration)
	{
		Guard.ArgumentNotNull(args);
		Guard.ArgumentNotNull(extensionRegistration);

		var builder = await TestApplication.CreateBuilderAsync(args);
		extensionRegistration(builder, args);

		var bannerCapability = new XunitBannerCapability();
		var trxCapability = new XunitTrxCapability();

		builder.CommandLine.AddProvider(() => new CommandLineOptionsProvider());
		builder.RegisterTestFramework(
			serviceProvider => new TestFrameworkCapabilities(bannerCapability, trxCapability),
			(capabilities, serviceProvider) =>
			{
				var logger = serviceProvider.GetLoggerFactory().CreateLogger("xUnit.net");
				var commandLineOptions = serviceProvider.GetCommandLineOptions();

				// Create the XunitProject and XunitProjectAssembly
				var project = new XunitProject();
				var testAssembly = Assembly.GetEntryAssembly() ?? throw new TestPipelineException("Could not find entry assembly");
				var assemblyFileName = Path.GetFullPath(testAssembly.GetSafeLocation() ?? throw new TestPipelineException("Test assembly must have an on-disk location"));
				var assemblyFolder = Path.GetDirectoryName(assemblyFileName) ?? throw new TestPipelineException("Test assembly must have an on-disk location");
				var targetFramework = testAssembly.GetTargetFramework();

				var configFileName = default(string);
				if (commandLineOptions.TryGetOptionArgumentList("xunit-config-filename", out var configFilenameArguments))
					configFileName = configFilenameArguments[0];

				var projectAssembly = new XunitProjectAssembly(project, assemblyFileName, new(3, targetFramework)) { Assembly = testAssembly, ConfigFileName = configFileName };
				ConfigReader_Json.Load(projectAssembly.Configuration, projectAssembly.AssemblyFileName, projectAssembly.ConfigFileName);
				project.Add(projectAssembly);

				// Read command line options
				CommandLineOptionsProvider.Parse(serviceProvider.GetConfiguration(), commandLineOptions, projectAssembly);

				// Get a diagnostic message sink
				var diagnosticMessages = projectAssembly.Configuration.DiagnosticMessagesOrDefault;
				var internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;
				var outputDevice = serviceProvider.GetOutputDevice();
				var diagnosticMessageSink = OutputDeviceDiagnosticMessageSink.TryCreate(logger, outputDevice, diagnosticMessages, internalDiagnosticMessages);

				// Use a runner logger which reports to the MTP logger, plus an option to enable output via IOutputDevice as well
				var runnerLogger = new OutputDeviceRunnerLogger(outputDevice, new LoggerRunnerLogger(logger), rawOnly: !commandLineOptions.IsOptionSet("xunit-info"));

				// Get the reporter and its message handler
				var supportAutoReporters = true;
				if (commandLineOptions.TryGetOptionArgumentList("auto-reporters", out var autoReportersArguments))
					supportAutoReporters = string.Equals(autoReportersArguments[0], "on", StringComparison.OrdinalIgnoreCase);

				var reporters = RegisteredRunnerReporters.Get(testAssembly, out _);
				var autoReporter = supportAutoReporters ? reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) : default;
				var reporter = autoReporter ?? reporters.FirstOrDefault(r => "default".Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase)) ?? new DefaultRunnerReporter();
				var reporterMessageHandler = reporter.CreateMessageHandler(runnerLogger, diagnosticMessageSink).SpinWait();

				return new TestPlatformTestFramework(runnerLogger, reporterMessageHandler, diagnosticMessageSink, projectAssembly, testAssembly, trxCapability, outputDevice);
			}
		);

		var app = await builder.BuildAsync();
		return await app.RunAsync();
	}
}
