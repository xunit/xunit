#pragma warning disable CA1044 // The write-only properties in this class cannot be converted to methods becuase of MSBuild task requirements
#pragma warning disable CA1721 // Properties with names that are confusing is okay because of MSBuild task requirements
#pragma warning disable CA1724 // The name of this type is a shipped contract (and part of the MSBuild UX)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Xunit.Runner.MSBuild;

/// <summary/>
public class xunit : MSBuildTask, ICancelableTask, IDisposable
{
	readonly CancellationTokenSource cancellationTokenSource = new();
	readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new();
	bool? diagnosticMessages;
	bool? failSkips;
	bool? failWarns;
	XunitFilters? filters;
	bool? internalDiagnosticMessages;
	IRunnerLogger? logger;
	readonly object logLock = new();
	bool? parallelizeAssemblies;
	bool? parallelizeTestCollections;
	bool? preEnumerateTheories;
	IMessageSink? reporterMessageHandler;
	bool? shadowCopy;
	bool? showLiveOutput;
	bool? stopOnFail;

	/// <summary/>
	public string? AppDomains { get; set; }

	/// <summary/>
	[Required]
	public ITaskItem[]? Assemblies { get; set; }

	/// <summary/>
	public ITaskItem? Ctrf { get; set; }

	/// <summary/>
	public string? Culture { get; set; }

	/// <summary/>
	public bool DiagnosticMessages { set => diagnosticMessages = value; }

	/// <summary/>
	public string? ExcludeTraits { get; set; }

	/// <summary/>
	public string? Explicit { get; set; }

	/// <summary/>
	[Output]
	public int ExitCode { get; protected set; }

	/// <summary/>
	public bool FailSkips { set => failSkips = value; }

	/// <summary/>
	public bool FailWarns { set => failWarns = value; }

	/// <summary/>
	protected XunitFilters Filters
	{
		get
		{
			if (filters is null)
			{
				var traitParser = new TraitParser(msg =>
				{
					lock (logLock)
						Log.LogWarning(msg);
				});

				filters = new XunitFilters();
				traitParser.Parse(IncludeTraits, filters.AddIncludedTraitFilter);
				traitParser.Parse(ExcludeTraits, filters.AddExcludedTraitFilter);
			}

			return filters;
		}
	}

	/// <summary/>
	public ITaskItem? Html { get; set; }

	/// <summary/>
	public bool IgnoreFailures { get; set; }

	/// <summary/>
	public string? IncludeTraits { get; set; }

	/// <summary/>
	public bool InternalDiagnosticMessages { set => internalDiagnosticMessages = value; }

	/// <summary/>
	public ITaskItem? JUnit { get; set; }

	/// <summary/>
	public string? MaxParallelThreads { get; set; }

	/// <summary/>
	public string? MethodDisplay { get; set; }

	/// <summary/>
	public string? MethodDisplayOptions { get; set; }

	/// <summary/>
	protected bool NeedsXml =>
		Xml is not null || XmlV1 is not null || Html is not null || NUnit is not null || JUnit is not null || Ctrf is not null || Trx is not null;

	/// <summary/>
	public bool NoAutoReporters { get; set; }

	/// <summary/>
	public bool NoLogo { get; set; }

	/// <summary/>
	public ITaskItem? NUnit { get; set; }

	/// <summary/>
	public string? ParallelAlgorithm { get; set; }

	/// <summary/>
	public bool ParallelizeAssemblies { set => parallelizeAssemblies = value; }

	/// <summary/>
	public bool ParallelizeTestCollections { set => parallelizeTestCollections = value; }

	/// <summary/>
	public bool PreEnumerateTheories { set => preEnumerateTheories = value; }

	/// <summary/>
	public string? Reporter { get; set; }

	/// <summary/>
	public bool ShadowCopy { set => shadowCopy = value; }

	/// <summary/>
	public bool ShowLiveOutput { set => showLiveOutput = value; }

	/// <summary/>
	public bool StopOnFail { set => stopOnFail = value; }

	/// <summary/>
	public ITaskItem? Trx { get; set; }

	/// <summary/>
	public string? WorkingFolder { get; set; }

	/// <summary/>
	public ITaskItem? Xml { get; set; }

	/// <summary/>
	public ITaskItem? XmlV1 { get; set; }

	/// <summary/>
	public void Cancel() =>
		cancellationTokenSource.Cancel();

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		cancellationTokenSource.Dispose();
	}

	/// <summary/>
	public override bool Execute() =>
		ExecuteAsync().GetAwaiter().GetResult();

	async Task<bool> ExecuteAsync()
	{
		Guard.ArgumentNotNull(Assemblies);

		RemotingUtility.CleanUpRegisteredChannels();

		XElement? assembliesElement = null;

		if (NeedsXml)
			assembliesElement = TransformFactory.CreateAssembliesElement();

		// Parse strings into structured values

		var appDomains = default(AppDomainSupport?);
		switch (AppDomains?.ToUpperInvariant())
		{
			case null:
				break;

			case "IFAVAILABLE":
				appDomains = AppDomainSupport.IfAvailable;
				break;

			case "TRUE":
			case "REQUIRED":
				appDomains = AppDomainSupport.Required;
				break;

			case "FALSE":
			case "DENIED":
				appDomains = AppDomainSupport.Denied;
				break;

			default:
				lock (logLock)
					Log.LogError("AppDomains value '{0}' is invalid: must be one of 'denied', 'ifAvailable', or 'required'", AppDomains);

				return false;
		}

		if (!TryParseOptionalEnum<ExplicitOption>(Explicit, "Explicit value '{0}' is invalid: must be one of 'on', 'off', or 'only'", out var explicitOption))
			return false;

		var maxParallelThreads = default(int?);
		if (MaxParallelThreads is not null)
		{
			switch (MaxParallelThreads.ToUpperInvariant())
			{
				case "DEFAULT":
				case "0":
					maxParallelThreads = Environment.ProcessorCount;
					break;

				case "UNLIMITED":
				case "-1":
					maxParallelThreads = -1;
					break;

				default:
					var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(MaxParallelThreads);
					if (match.Success && decimal.TryParse(match.Groups[1].Value, out var maxThreadMultiplier))
						maxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
					else if (int.TryParse(MaxParallelThreads, out var threadValue) && threadValue > 0)
						maxParallelThreads = threadValue;
					else
					{
						lock (logLock)
							Log.LogError("MaxParallelThreads value '{0}' is invalid: must be one of 'default', 'unlimited', a positive number, or a multiplier in the form of '0.0x'", MaxParallelThreads);

						return false;
					}

					break;
			}
		}

		if (!TryParseOptionalEnum<TestMethodDisplay>(MethodDisplay, "MethodDisplay value '{0}' is invalid: must be one of 'classAndMethod' or 'method'", out var methodDisplay))
			return false;

		if (!TryParseOptionalEnum<TestMethodDisplayOptions>(MethodDisplayOptions, "MethodDisplayOptions value '{0}' is invalid: must be one of 'all', 'none', or a comma-separated list of one or more of 'replacePeriodWithComma', 'replaceUnderscoreWithSpace', 'useOperatorMonikers', or 'useEscapeSequences'", out var methodDisplayOptions))
			return false;

		if (!TryParseOptionalEnum<ParallelAlgorithm>(ParallelAlgorithm, "ParallelAlgorithm value '{0}' is invalid: must be one of 'aggressive' or 'conservative'", out var parallelAlgorithm))
			return false;

		var originalWorkingFolder = Directory.GetCurrentDirectory();
		await using var globalDiagnosticsMessageSink = MSBuildDiagnosticMessageSink.TryCreate(Log, logLock, diagnosticMessages ?? false, internalDiagnosticMessages ?? false);

		using (AssemblyHelper.SubscribeResolveForAssembly(typeof(xunit), globalDiagnosticsMessageSink))
		{
			var reporter = GetReporter();
			if (reporter is null)
				return false;

			logger = new MSBuildLogger(Log);
			reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticsMessageSink);

			if (!NoLogo)
				lock (logLock)
					Log.LogMessage(MessageImportance.High, "xUnit.net v3 MSBuild Runner v{0} ({1}-bit {2})", ThisAssembly.AssemblyInformationalVersion, IntPtr.Size * 8, RuntimeInformation.FrameworkDescription);

			var project = new XunitProject();
			foreach (var assembly in Assemblies)
			{
				var assemblyFileName = assembly.GetMetadata("FullPath");
				if (!File.Exists(assemblyFileName))
				{
					lock (logLock)
						Log.LogError("Assembly '{0}' does not exist", assemblyFileName);
					return false;
				}

				var configFileName = assembly.GetMetadata("ConfigFile");
				if (configFileName is not null && configFileName.Length == 0)
					configFileName = null;
				if (configFileName is not null && !File.Exists(configFileName))
				{
					lock (logLock)
						Log.LogError("Configuration file '{0}' does not exist", configFileName);
					return false;
				}

				var metadata = AssemblyUtility.GetAssemblyMetadata(assemblyFileName);
				if (metadata is null)
				{
					lock (logLock)
						Log.LogError("Assembly '{0}' is not a valid .NET assembly", assemblyFileName);
					return false;
				}

#if NETCOREAPP
				if (metadata.XunitVersion < 3)
				{
					lock (logLock)
						Log.LogError("Assembly '{0}' is not supported (only v3+ tests are supported in dotnet msbuild)", assemblyFileName);
					return false;
				}
#endif

				var projectAssembly = new XunitProjectAssembly(project, assemblyFileName, metadata) { ConfigFileName = configFileName };

				var warnings = new List<string>();
				ConfigReader.Load(projectAssembly.Configuration, assemblyFileName, configFileName, warnings);

				foreach (var warning in warnings)
					logger.LogWarning(warning);

				if (appDomains.HasValue)
					projectAssembly.Configuration.AppDomain = appDomains;
				if (Culture is not null)
					projectAssembly.Configuration.Culture = Culture.ToUpperInvariant() switch
					{
						"DEFAULT" => null,
						"INVARIANT" => string.Empty,
						_ => Culture,
					};
				if (diagnosticMessages.HasValue)
					projectAssembly.Configuration.DiagnosticMessages = diagnosticMessages;
				if (explicitOption.HasValue)
					projectAssembly.Configuration.ExplicitOption = explicitOption;
				if (internalDiagnosticMessages.HasValue)
					projectAssembly.Configuration.InternalDiagnosticMessages = internalDiagnosticMessages;
				if (failSkips.HasValue)
					projectAssembly.Configuration.FailSkips = failSkips;
				if (failWarns.HasValue)
					projectAssembly.Configuration.FailTestsWithWarnings = failWarns;
				if (maxParallelThreads.HasValue)
					projectAssembly.Configuration.MaxParallelThreads = maxParallelThreads;
				if (methodDisplay.HasValue)
					projectAssembly.Configuration.MethodDisplay = methodDisplay;
				if (methodDisplayOptions.HasValue)
					projectAssembly.Configuration.MethodDisplayOptions = methodDisplayOptions;
				if (parallelAlgorithm.HasValue)
					projectAssembly.Configuration.ParallelAlgorithm = parallelAlgorithm;
				if (parallelizeTestCollections.HasValue)
					projectAssembly.Configuration.ParallelizeTestCollections = parallelizeTestCollections;
				if (preEnumerateTheories.HasValue)
					projectAssembly.Configuration.PreEnumerateTheories = preEnumerateTheories;
				if (shadowCopy.HasValue)
					projectAssembly.Configuration.ShadowCopy = shadowCopy;
				if (showLiveOutput.HasValue)
					projectAssembly.Configuration.ShowLiveOutput = showLiveOutput;
				if (stopOnFail.HasValue)
					projectAssembly.Configuration.StopOnFail = stopOnFail;

				project.Add(projectAssembly);
			}

			if (WorkingFolder is not null)
				Directory.SetCurrentDirectory(WorkingFolder);

			var clockTime = Stopwatch.StartNew();

			if (!parallelizeAssemblies.HasValue)
				parallelizeAssemblies = project.Assemblies.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);

			if (parallelizeAssemblies.GetValueOrDefault())
			{
				var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(assembly, appDomains).AsTask()));
				var results = await Task.WhenAll(tasks);
				foreach (var assemblyElement in results.WhereNotNull())
					assembliesElement!.Add(assemblyElement);
			}
			else
			{
				foreach (var assembly in project.Assemblies)
				{
					var assemblyElement = await ExecuteAssembly(assembly, appDomains);
					if (assemblyElement is not null)
						assembliesElement!.Add(assemblyElement);
				}
			}

			clockTime.Stop();

			if (!completionMessages.IsEmpty)
			{
				var summaries = new TestExecutionSummaries { ElapsedClockTime = clockTime.Elapsed };
				foreach (var completionMessage in completionMessages.OrderBy(kvp => kvp.Key))
					summaries.Add(completionMessage.Key, completionMessage.Value);
				reporterMessageHandler.OnMessage(summaries);
			}
		}

		Directory.SetCurrentDirectory(WorkingFolder ?? originalWorkingFolder);

		if (NeedsXml && assembliesElement is not null)
		{
			TransformFactory.FinishAssembliesElement(assembliesElement);

			if (Xml is not null)
				TransformFactory.Transform("xml", assembliesElement, Xml.GetMetadata("FullPath"));

			if (XmlV1 is not null)
				TransformFactory.Transform("xmlv1", assembliesElement, XmlV1.GetMetadata("FullPath"));

			if (Html is not null)
				TransformFactory.Transform("html", assembliesElement, Html.GetMetadata("FullPath"));

			if (NUnit is not null)
				TransformFactory.Transform("nunit", assembliesElement, NUnit.GetMetadata("FullPath"));

			if (JUnit is not null)
				TransformFactory.Transform("junit", assembliesElement, JUnit.GetMetadata("FullPath"));

			if (Ctrf is not null)
				TransformFactory.Transform("ctrf", assembliesElement, Ctrf.GetMetadata("FullPath"));

			if (Trx is not null)
				TransformFactory.Transform("trx", assembliesElement, Trx.GetMetadata("FullPath"));
		}

		// ExitCode is set to 1 for test failures and -1 for Exceptions.
		return ExitCode == 0 || (ExitCode == 1 && IgnoreFailures);
	}

	/// <summary/>
	protected virtual async ValueTask<XElement?> ExecuteAssembly(
		XunitProjectAssembly assembly,
		AppDomainSupport? appDomains)
	{
		Guard.ArgumentNotNull(assembly);

		if (cancellationTokenSource.IsCancellationRequested)
			return null;

		Guard.NotNull("Runner is misconfigured ('reporterMessageHandler' is null)", reporterMessageHandler);

		var assemblyElement = NeedsXml ? new XElement("assembly") : null;

		try
		{
			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);

			var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFileName)!;
			await using var diagnosticMessageSink = MSBuildDiagnosticMessageSink.TryCreate(Log, logLock, diagnosticMessages ?? assembly.Configuration.DiagnosticMessagesOrDefault, internalDiagnosticMessages ?? assembly.Configuration.InternalDiagnosticMessagesOrDefault, assemblyDisplayName);
			var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			await using var controller =
				XunitFrontController.Create(assembly, diagnosticMessageSink: diagnosticMessageSink)
					?? throw new ArgumentException("not an xUnit.net test assembly: {0}", assembly.AssemblyFileName);

			var appDomain = (controller.CanUseAppDomains, appDomainSupport) switch
			{
				(false, AppDomainSupport.Required) => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "AppDomains were required but assembly '{0}' does not support them", assembly.AssemblyFileName)),
				(false, _) => AppDomainOption.NotAvailable,
				(true, AppDomainSupport.Denied) => AppDomainOption.Disabled,
				(true, _) => AppDomainOption.Enabled,
			};

			var sinkOptions = new ExecutionSinkOptions
			{
				AssemblyElement = assemblyElement,
				CancelThunk = () => cancellationTokenSource.IsCancellationRequested,
				DiagnosticMessageSink = diagnosticMessageSink,
				FailSkips = assembly.Configuration.FailSkipsOrDefault,
				FailWarn = assembly.Configuration.FailTestsWithWarningsOrDefault,
				FinishedCallback = summary => completionMessages.TryAdd(controller.TestAssemblyUniqueID, summary),
				LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
			};

			using var resultsSink = new ExecutionSink(assembly, discoveryOptions, executionOptions, appDomain, assembly.Configuration.ShadowCopyOrDefault, reporterMessageHandler, sinkOptions);
			var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, assembly.Configuration.Filters);
			controller.FindAndRun(resultsSink, settings);
			resultsSink.Finished.WaitOne();

			if (resultsSink.ExecutionSummary.Failed != 0 || resultsSink.ExecutionSummary.Errors != 0)
			{
				ExitCode = 1;
				if (executionOptions.GetStopOnTestFailOrDefault())
				{
					lock (logLock)
						Log.LogMessage(MessageImportance.High, "Cancelling due to test failure...");

					Cancel();
				}
			}
		}
		catch (Exception ex)
		{
			var e = ex;

			lock (logLock)
				while (e is not null)
				{
					Log.LogError("{0}: {1}", e.GetType().SafeName(), e.Message);

					if (e.StackTrace is not null)
						foreach (var stackLine in e.StackTrace.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries))
							Log.LogError(stackLine);

					e = e.InnerException;
				}

			ExitCode = -1;
		}

		return assemblyElement;
	}

	/// <summary/>
	protected virtual IReadOnlyList<IRunnerReporter> GetAvailableRunnerReporters() =>
		RegisteredRunnerReporters.Get(typeof(xunit).Assembly, out _);

	/// <summary/>
	protected IRunnerReporter? GetReporter()
	{
		var reporters = GetAvailableRunnerReporters();
		IRunnerReporter? reporter = null;
		if (!NoAutoReporters)
			reporter = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled);

		if (reporter is null && !string.IsNullOrWhiteSpace(Reporter))
		{
			reporter = reporters.FirstOrDefault(r => string.Equals(r.RunnerSwitch, Reporter, StringComparison.OrdinalIgnoreCase));
			if (reporter is null)
			{
#pragma warning disable CA1308 // The switch list is lowercased because it's presented in the UI that way
				var switchableReporters =
					reporters
						.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch))
						.Select(r => r.RunnerSwitch!.ToLowerInvariant())
						.OrderBy(x => x)
						.ToList();
#pragma warning restore CA1308

				lock (logLock)
					if (switchableReporters.Count == 0)
						Log.LogError("Reporter value '{0}' is invalid. There are no available reporters.", Reporter);
					else
						Log.LogError("Reporter value '{0}' is invalid. Available reporters: {1}", Reporter, string.Join(", ", switchableReporters));

				return null;
			}
		}

		return reporter ?? new DefaultRunnerReporter();
	}

	bool TryParseOptionalEnum<TEnum>(
		string? value,
		string failureMessageFormat,
		out TEnum? result)
			where TEnum : struct
	{
		if (value == null)
		{
			result = default;
			return true;
		}

		if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var resultValue))
		{
			result = resultValue;
			return true;
		}

		lock (logLock)
			Log.LogError(failureMessageFormat, value);

		result = default;
		return false;
	}
}
