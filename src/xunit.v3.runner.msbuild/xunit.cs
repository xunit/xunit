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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Xunit.Runner.MSBuild;

public class xunit : MSBuildTask, ICancelableTask
{
	volatile bool cancel;
	readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new();
	bool? diagnosticMessages;
	bool? failSkips;
	bool? failWarns;
	XunitFilters? filters;
	bool? internalDiagnosticMessages;
	IRunnerLogger? logger;
	readonly object logLock = new();
	int? maxThreadCount;
	bool? parallelizeAssemblies;
	bool? parallelizeTestCollections;
	bool? preEnumerateTheories;
	_IMessageSink? reporterMessageHandler;
	bool? shadowCopy;
	bool? stopOnFail;

	public string? AppDomains { get; set; }

	[Required]
	public ITaskItem[]? Assemblies { get; set; }

	public string? Culture { get; set; }

	public bool DiagnosticMessages { set { diagnosticMessages = value; } }

	public string? ExcludeTraits { get; set; }

	public string? Explicit { get; set; }

	[Output]
	public int ExitCode { get; protected set; }

	public bool FailSkips { set { failSkips = value; } }

	public bool FailWarns { set { failWarns = value; } }

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
				traitParser.Parse(IncludeTraits, filters.IncludedTraits);
				traitParser.Parse(ExcludeTraits, filters.ExcludedTraits);
			}

			return filters;
		}
	}

	public ITaskItem? Html { get; set; }

	public bool IgnoreFailures { get; set; }

	public string? IncludeTraits { get; set; }

	public bool InternalDiagnosticMessages { set { internalDiagnosticMessages = value; } }

	public ITaskItem? JUnit { get; set; }

	public string? MaxParallelThreads { get; set; }

	protected bool NeedsXml =>
		Xml is not null || XmlV1 is not null || Html is not null || NUnit is not null || JUnit is not null;

	public bool NoAutoReporters { get; set; }

	public bool NoLogo { get; set; }

	public ITaskItem? NUnit { get; set; }

	public bool ParallelizeAssemblies { set { parallelizeAssemblies = value; } }

	public bool ParallelizeTestCollections { set { parallelizeTestCollections = value; } }

	public bool PreEnumerateTheories { set { preEnumerateTheories = value; } }

	public string? Reporter { get; set; }

	public bool ShadowCopy { set { shadowCopy = value; } }

	public bool StopOnFail { set { stopOnFail = value; } }

	public string? WorkingFolder { get; set; }

	public ITaskItem? Xml { get; set; }

	public ITaskItem? XmlV1 { get; set; }

	public void Cancel()
	{
		cancel = true;
	}

	public override bool Execute() =>
		ExecuteAsync().GetAwaiter().GetResult();

	async Task<bool> ExecuteAsync()
	{
		Guard.ArgumentNotNull(Assemblies);

		RemotingUtility.CleanUpRegisteredChannels();

		XElement? assembliesElement = null;

		if (NeedsXml)
			assembliesElement = TransformFactory.CreateAssembliesElement();

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
					Log.LogError("AppDomains value '{0}' is invalid: must be one of 'IfAvailable', 'Required', or 'Denied'", AppDomains);

				return false;
		}

		switch (MaxParallelThreads)
		{
			case null:
			case "default":
			case "0":
				break;

			case "unlimited":
			case "-1":
				maxThreadCount = -1;
				break;

			default:
				var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(MaxParallelThreads);
				if (match.Success && decimal.TryParse(match.Groups[1].Value, out var maxThreadMultiplier))
					maxThreadCount = (int)(maxThreadMultiplier * Environment.ProcessorCount);
				else if (int.TryParse(MaxParallelThreads, out var threadValue) && threadValue > 0)
					maxThreadCount = threadValue;
				else
				{
					lock (logLock)
						Log.LogError("MaxParallelThreads value '{0}' is invalid: must be 'default', 'unlimited', a positive number, or a multiplier in the form of '0.0x'", MaxParallelThreads);

					return false;
				}

				break;
		}

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
				var configFileName = assembly.GetMetadata("ConfigFile");
				if (configFileName is not null && configFileName.Length == 0)
					configFileName = null;

				var targetFramework = AssemblyUtility.GetTargetFramework(assemblyFileName);
				var projectAssembly = new XunitProjectAssembly(project)
				{
					AssemblyFileName = assemblyFileName,
					ConfigFileName = configFileName,
					TargetFramework = targetFramework
				};

				ConfigReader.Load(projectAssembly.Configuration, assemblyFileName, configFileName);

				if (Culture is not null)
					projectAssembly.Configuration.Culture = Culture switch
					{
						"default" => null,
						"invariant" => string.Empty,
						_ => Culture,
					};

				if (Explicit is not null)
					projectAssembly.Configuration.ExplicitOption = Explicit.ToUpperInvariant() switch
					{
						"OFF" => ExplicitOption.Off,
						"ON" => ExplicitOption.On,
						"ONLY" => ExplicitOption.Only,
						_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid value for Explicit ('{0}'); valid values are 'off', 'on', and 'only'", Explicit)),
					};

				if (shadowCopy.HasValue)
					projectAssembly.Configuration.ShadowCopy = shadowCopy;

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
		}

		// ExitCode is set to 1 for test failures and -1 for Exceptions.
		return ExitCode == 0 || (ExitCode == 1 && IgnoreFailures);
	}

	protected virtual async ValueTask<XElement?> ExecuteAssembly(
		XunitProjectAssembly assembly,
		AppDomainSupport? appDomains)
	{
		Guard.ArgumentNotNull(assembly);

		if (cancel)
			return null;

		Guard.NotNull("Runner is misconfigured ('reporterMessageHandler' is null)", reporterMessageHandler);

		var assemblyElement = NeedsXml ? new XElement("assembly") : null;

		try
		{
			if (preEnumerateTheories.HasValue)
				assembly.Configuration.PreEnumerateTheories = preEnumerateTheories.Value;
			if (diagnosticMessages.HasValue)
				assembly.Configuration.DiagnosticMessages = diagnosticMessages.Value;
			if (internalDiagnosticMessages.HasValue)
				assembly.Configuration.InternalDiagnosticMessages = internalDiagnosticMessages.Value;
			if (failSkips.HasValue)
				assembly.Configuration.FailSkips = failSkips.Value;
			if (failWarns.HasValue)
				assembly.Configuration.FailWarns = failWarns.Value;

			if (appDomains.HasValue)
				assembly.Configuration.AppDomain = appDomains;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);
			if (maxThreadCount.HasValue && maxThreadCount.Value > -1)
				executionOptions.SetMaxParallelThreads(maxThreadCount);
			if (parallelizeTestCollections.HasValue)
				executionOptions.SetDisableParallelization(!parallelizeTestCollections);
			if (stopOnFail.HasValue)
				executionOptions.SetStopOnTestFail(stopOnFail);

			var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFileName)!;
			await using var diagnosticMessageSink = MSBuildDiagnosticMessageSink.TryCreate(Log, logLock, diagnosticMessages ?? assembly.Configuration.DiagnosticMessagesOrDefault, internalDiagnosticMessages ?? assembly.Configuration.InternalDiagnosticMessagesOrDefault, assemblyDisplayName);
			var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
			var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			await using var controller = XunitFrontController.ForDiscoveryAndExecution(assembly, diagnosticMessageSink: diagnosticMessageSink);

			var appDomain = (controller.CanUseAppDomains, appDomainSupport) switch
			{
				(false, AppDomainSupport.Required) => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "AppDomains were required but assembly '{0}' does not support them", assembly.AssemblyFileName)),
				(false, _) => AppDomainOption.NotAvailable,
				(true, AppDomainSupport.Denied) => AppDomainOption.Disabled,
				(true, _) => AppDomainOption.Enabled,
			};

			IExecutionSink resultsSink = new DelegatingSummarySink(
				assembly,
				discoveryOptions,
				executionOptions,
				appDomain,
				shadowCopy,
				reporterMessageHandler,
				() => cancel,
				(summary, _) => completionMessages.TryAdd(controller.TestAssemblyUniqueID, summary)
			);

			if (assemblyElement is not null)
				resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
			if (longRunningSeconds > 0 && diagnosticMessageSink is not null)
				resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink);
			if (assembly.Configuration.FailSkipsOrDefault)
				resultsSink = new DelegatingFailSkipSink(resultsSink);
			if (assembly.Configuration.FailWarnsOrDefault)
				resultsSink = new DelegatingFailWarnSink(resultsSink);

			using (resultsSink)
			{
				var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, assembly.Configuration.Filters);
				controller.FindAndRun(resultsSink, settings);
				resultsSink.Finished.WaitOne();

				if (resultsSink.ExecutionSummary.Failed != 0 || resultsSink.ExecutionSummary.Errors != 0)
				{
					ExitCode = 1;
					if (executionOptions.GetStopOnTestFailOrDefault())
					{
						lock (logLock)
							Log.LogMessage(MessageImportance.High, "Canceling due to test failure...");

						Cancel();
					}
				}
			}
		}
		catch (Exception ex)
		{
			var e = ex;

			lock (logLock)
				while (e is not null)
				{
					Log.LogError("{0}: {1}", e.GetType().FullName, e.Message);

					if (e.StackTrace is not null)
						foreach (var stackLine in e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
							Log.LogError(stackLine);

					e = e.InnerException;
				}

			ExitCode = -1;
		}

		return assemblyElement;
	}

	protected virtual List<IRunnerReporter> GetAvailableRunnerReporters()
	{
		var runnerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
		if (runnerPath is null)
			return new List<IRunnerReporter>();

		var result = RunnerReporterUtility.GetAvailableRunnerReporters(runnerPath, includeEmbeddedReporters: true, out var messages);

		if (messages.Count != 0)
			lock (logLock)
				foreach (var message in messages)
					Log.LogWarning(message);

		return result;
	}

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
}
