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
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed. The only guaranteed stable API
/// in this class is <see cref="RunAsync(string[], Action{ITestApplicationBuilder, string[]})"/>.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TestPlatformTestFramework :
	OutputDeviceDataProducerBase, ITestPlatformTestFramework, IDataProducer
{
	readonly IMessageSink? diagnosticMessageSink;
	readonly IOutputDevice outputDevice;
	readonly XunitProjectAssembly projectAssembly;
	readonly ConcurrentDictionary<SessionUid, (CountdownEvent OperationCounter, IRunnerReporterMessageHandler MessageHandler)> sessions = new();
	readonly IRunnerLogger runnerLogger;
	readonly IRunnerReporter runnerReporter;
	readonly bool serverMode;
	readonly Assembly testAssembly;
	readonly XunitTrxCapability trxCapability;

	/// <summary/>
	protected TestPlatformTestFramework(
		IRunnerLogger runnerLogger,
		IRunnerReporter runnerReporter,
		IMessageSink? diagnosticMessageSink,
		XunitProjectAssembly projectAssembly,
		Assembly testAssembly,
		XunitTrxCapability trxCapability,
		IOutputDevice outputDevice,
		bool serverMode) :
			base("test framework", "30ea7c6e-dd24-4152-a360-1387158cd41d")
	{
		this.runnerLogger = runnerLogger;
		this.runnerReporter = runnerReporter;
		this.diagnosticMessageSink = diagnosticMessageSink;
		this.projectAssembly = projectAssembly;
		this.testAssembly = testAssembly;
		this.trxCapability = trxCapability;
		this.outputDevice = outputDevice;
		this.serverMode = serverMode;

		SerializationHelper.Instance.AddRegisteredSerializers(testAssembly);
	}

	/// <inheritdoc/>
	public Type[] DataTypesProduced =>
		[typeof(SessionFileArtifact)];

	/// <summary/>
	public async Task<CloseTestSessionResult> CloseTestSession(SessionUid sessionUid)
	{
		if (!sessions.TryRemove(sessionUid, out var session))
			return new CloseTestSessionResult { IsSuccess = false, ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Attempted to close unknown session UID '{0}'", sessionUid.Value) };

		session.OperationCounter.Signal();
		session.OperationCounter.Wait();
		session.OperationCounter.SafeDispose();

		await session.MessageHandler.SafeDisposeAsync();

		return new CloseTestSessionResult { IsSuccess = true };
	}

	Task<CloseTestSessionResult> ITestPlatformTestFramework.CloseTestSessionAsync(CloseTestSessionContext context)
	{
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariables.TestingPlatformDebug)))
			Debugger.Launch();

		return CloseTestSession(Guard.ArgumentNotNull(context).SessionUid);
	}

	/// <summary/>
	public async Task<CreateTestSessionResult> CreateTestSession(SessionUid sessionUid)
	{
		var countdownEvent = new CountdownEvent(1);
		var reporterMessageHandler = await runnerReporter.CreateMessageHandler(runnerLogger, diagnosticMessageSink);

		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Call to {0}.CreateMessageHandler returned a null value", runnerReporter.GetType().SafeName()), reporterMessageHandler);

		if (!sessions.TryAdd(sessionUid, (countdownEvent, reporterMessageHandler)))
		{
			countdownEvent.SafeDispose();
			await reporterMessageHandler.SafeDisposeAsync();

			return new CreateTestSessionResult { IsSuccess = false, ErrorMessage = string.Format(CultureInfo.CurrentCulture, "Attempted to reuse session UID '{0}' already in progress", sessionUid.Value) };
		}

		if (!projectAssembly.Project.Configuration.NoLogoOrDefault)
			runnerLogger.LogImportantMessage(ProjectAssemblyRunner.Banner);

		return new CreateTestSessionResult { IsSuccess = true };
	}

	Task<CreateTestSessionResult> ITestPlatformTestFramework.CreateTestSessionAsync(CreateTestSessionContext context)
	{
		if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvironmentVariables.TestingPlatformDebug)))
			Debugger.Launch();

		return CreateTestSession(Guard.ArgumentNotNull(context).SessionUid);
	}

	async Task ITestPlatformTestFramework.ExecuteRequestAsync(ExecuteRequestContext context)
	{
		Guard.ArgumentNotNull(context);

		if (context.Request is DiscoverTestExecutionRequest discoverRequest)
			await OnDiscover(context.Request.Session.SessionUid, context.MessageBus, context.Complete, context.CancellationToken);
		else if (context.Request is RunTestExecutionRequest executionRequest)
			await OnExecute(context.Request.Session.SessionUid, executionRequest.Filter, context.MessageBus, context.Complete, context.CancellationToken);
	}

	/// <summary/>
	public ValueTask OnDiscover(
		SessionUid sessionUid,
		ITestPlatformMessageBus messageBus,
		Action operationComplete,
		CancellationToken cancellationToken)
	{
		if (!sessions.TryGetValue(sessionUid, out var session))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Attempted to run discovery request against unknown session UID '{0}'", sessionUid.Value), nameof(sessionUid));

		return OnRequest(session.OperationCounter, operationComplete, async (projectRunner, pipelineStartup) =>
		{
			// Default to true for Testing Platform
			// TODO: We'd prefer true for Test Explorer and false for `dotnet test`
			projectAssembly.Configuration.PreEnumerateTheories ??= true;

			var messageHandler = new TestPlatformDiscoveryMessageSink(session.MessageHandler, projectAssembly.Assembly!.FullName!, sessionUid, messageBus, cancellationToken);
			await projectRunner.Discover(projectAssembly, pipelineStartup, messageHandler);
		}, cancellationToken);
	}

	/// <summary/>
	public ValueTask OnExecute(
		SessionUid sessionUid,
		ITestExecutionFilter? filter,
		ITestPlatformMessageBus messageBus,
		Action operationComplete,
		CancellationToken cancellationToken)
	{
		if (!sessions.TryGetValue(sessionUid, out var session))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Attempted to run execution request against unknown session UID '{0}'", sessionUid.Value), nameof(sessionUid));

		return OnRequest(session.OperationCounter, operationComplete, async (projectRunner, pipelineStartup) =>
		{
			if (Debugger.IsAttached)
				await outputDevice.DisplayAsync(this, ToMessageWithColor("* Note: Long running test detection and test timeouts are disabled due to an attached debugger *" + Environment.NewLine, ConsoleColor.Yellow), cancellationToken);

			var testCaseIDsToRun = filter switch
			{
				TestNodeUidListFilter filter => filter.TestNodeUids.Select(u => u.Value).ToHashSet(StringComparer.OrdinalIgnoreCase),
				_ => null,
			};

			// Default to true for Test Explorer, false otherwise
			projectAssembly.Configuration.PreEnumerateTheories ??= serverMode;

			// If the user asked to run specific tests, then we auto-enable explicit support since
			// the Test Explorer UX has no way to turn support for explicit tests on/off
			if (serverMode && testCaseIDsToRun is not null)
				projectAssembly.AutoEnableExplicit = true;

			// If the user wants live output, we'll turn it off in the configuration (so the default reporter doesn't
			// report it) and instead tell the message sink to display it.
			var showLiveOutput = projectAssembly.Configuration.ShowLiveOutputOrDefault;
			projectAssembly.Configuration.ShowLiveOutput = false;

			var messageHandler = new TestPlatformExecutionMessageSink(session.MessageHandler, sessionUid, messageBus, trxCapability, outputDevice, showLiveOutput, serverMode, cancellationToken);
			await projectRunner.Run(projectAssembly, messageHandler, diagnosticMessageSink, runnerLogger, pipelineStartup, testCaseIDsToRun);

			foreach (var output in projectAssembly.Project.Configuration.Output)
				await messageBus.PublishAsync(this, new SessionFileArtifact(sessionUid, new FileInfo(output.Value), Path.GetFileNameWithoutExtension(output.Value)));
		}, cancellationToken);
	}

	async ValueTask OnRequest(
		CountdownEvent operationCounter,
		Action operationComplete,
		Func<ProjectAssemblyRunner, ITestPipelineStartup?, ValueTask> callback,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(operationComplete);
		Guard.ArgumentNotNull(callback);

		operationCounter.AddCount();

		try
		{
			var pipelineStartup = await ProjectAssemblyRunner.InvokePipelineStartup(testAssembly, diagnosticMessageSink);

			try
			{
				using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				var projectRunner = new ProjectAssemblyRunner(testAssembly, AutomatedMode.Off, NullSourceInformationProvider.Instance, cancellationTokenSource);
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
	/// <returns>The return code to be returned from <c>Main</c></returns>
	public static async Task<int> RunAsync(
		string[] args,
		Action<ITestApplicationBuilder, string[]> extensionRegistration)
	{
		Guard.ArgumentNotNull(args);
		Guard.ArgumentNotNull(extensionRegistration);

		using var _ = new TraceAssertOverrideListener();

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
				var serverMode = commandLineOptions.IsOptionSet("server");

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

				// Read configuration and command line options
				var configuration = serviceProvider.GetConfiguration();
				TestConfig.Parse(configuration, projectAssembly);
				CommandLineOptionsProvider.Parse(configuration, commandLineOptions, projectAssembly);

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

				var reporters = RegisteredRunnerReporters.Get(testAssembly, out var _1);
				var autoReporter = supportAutoReporters ? reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) : default;
				var reporter = autoReporter ?? reporters.FirstOrDefault(r => "default".Equals(r.RunnerSwitch, StringComparison.OrdinalIgnoreCase)) ?? new DefaultRunnerReporter();

				return new TestPlatformTestFramework(runnerLogger, reporter, diagnosticMessageSink, projectAssembly, testAssembly, trxCapability, outputDevice, serverMode);
			}
		);

		var app = await builder.BuildAsync();
		return await app.RunAsync();
	}
}
