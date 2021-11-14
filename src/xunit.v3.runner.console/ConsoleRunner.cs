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
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

namespace Xunit.Runner.SystemConsole;

class ConsoleRunner
{
	readonly string[] args;
	volatile bool cancel;
	readonly object consoleLock = new();
	readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new();
	bool failed;
	IRunnerLogger? logger;

	public ConsoleRunner(
		string[] args,
		object? consoleLock = null)
	{
		this.args = Guard.ArgumentNotNull(args);
		this.consoleLock = consoleLock ?? new object();
	}

	public async ValueTask<int> EntryPoint()
	{
		var globalInternalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var runnerFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			var commandLine = new CommandLine(runnerFolder, args);

			if (args.Length == 0 || commandLine.HelpRequested)
			{
				PrintHeader();

				var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

				Console.WriteLine("Copyright (C) .NET Foundation.");
				Console.WriteLine();
				Console.WriteLine($"usage: {executableName} <assemblyFile> [configFile] [assemblyFile [configFile]...] [options] [reporter] [resultFormat filename [...]]");
				Console.WriteLine();
				Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
				Console.WriteLine("      XML is supported for v1 and v2 only, on .NET Framework only");
				Console.WriteLine("      JSON is supported for v2 and later, on all supported plaforms");

				commandLine.PrintUsage();

				return 2;
			}

			var project = commandLine.Parse();
			if (project.Assemblies.Count == 0)
				throw new ArgumentException("must specify at least one assembly");

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Console.CancelKeyPress += (sender, e) =>
			{
				if (!cancel)
				{
					Console.WriteLine("Canceling... (Press Ctrl+C again to terminate)");
					cancel = true;
					e.Cancel = true;
				}
			};

			if (project.Configuration.PauseOrDefault)
			{
				Console.Write("Press any key to start execution...");
				Console.ReadKey(true);
				Console.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			// We will enable "global" internal diagnostic messages if any test assembly wanted them
			globalInternalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);
			noColor = project.Configuration.NoColorOrDefault;
			logger = new ConsoleRunnerLogger(!noColor, consoleLock);
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.ForInternalDiagnostics(consoleLock, globalInternalDiagnosticMessages, noColor);
			var reporter = project.RunnerReporter;
			var reporterMessageHandler = await reporter.CreateMessageHandler(logger, diagnosticMessageSink);

			if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
				PrintHeader();

			var failCount = 0;

			if (project.Configuration.List != null)
				await ListProject(project);
			else
				failCount = await RunProject(project, reporterMessageHandler);

			if (cancel)
				return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C

			if (project.Configuration.WaitOrDefault)
			{
				Console.WriteLine();
				Console.Write("Press any key to continue...");
				Console.ReadKey();
				Console.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				ConsoleHelper.SetForegroundColor(ConsoleColor.Red);

			Console.WriteLine($"error: {ex.Message}");

			if (globalInternalDiagnosticMessages)
			{
				if (!noColor)
					ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

				Console.WriteLine(ex.StackTrace);
			}

			return ex is ArgumentException ? 3 : 4;
		}
		finally
		{
			if (!noColor)
				ConsoleHelper.ResetColor();
		}
	}

	void OnUnhandledException(
		object sender,
		UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
			Console.WriteLine(ex.ToString());
		else
			Console.WriteLine("Error of unknown type thrown in application domain");

		Environment.Exit(1);
	}

	void PrintHeader()
	{
#if NET472
		var buildTarget = $"net472";
#elif NET48
			var buildTarget = $"net48";
#else
#error Unknown target framework
#endif

#if BUILD_X86
			buildTarget += "/x86";
#else
		buildTarget += "/AnyCPU";
#endif

		Console.WriteLine($"xUnit.net v3 Console Runner v{ThisAssembly.AssemblyInformationalVersion} [{buildTarget}] ({IntPtr.Size * 8}-bit {RuntimeInformation.FrameworkDescription})");
	}

	async ValueTask ListProject(XunitProject project)
	{
		var (listOption, listFormat) = project.Configuration.List!.Value;
		var testCasesByAssembly = new Dictionary<string, List<_TestCaseDiscovered>>();

		foreach (var assembly in project.Assemblies)
		{
			var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);

			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);

			var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
			var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			using var _ = AssemblyHelper.SubscribeResolveForAssembly(assemblyFileName);
			await using var controller = XunitFrontController.ForDiscoveryAndExecution(assembly);

			var discoverySink = new TestDiscoverySink(() => cancel);

			var settings = new FrontControllerFindSettings(discoveryOptions, assembly.Configuration.Filters);
			controller.Find(discoverySink, settings);
			discoverySink.Finished.WaitOne();

			testCasesByAssembly.Add(assemblyFileName, discoverySink.TestCases);
		}

		ConsoleProjectLister.List(testCasesByAssembly, listOption, listFormat);
	}

	async ValueTask<int> RunProject(
		XunitProject project,
		_IMessageSink reporterMessageHandler)
	{
		XElement? assembliesElement = null;
		var clockTime = Stopwatch.StartNew();
		var xmlTransformers = TransformFactory.GetXmlTransformers(project);
		var needsXml = xmlTransformers.Count > 0;
		// TODO: Parallelize the ones that will parallelize, and then run the rest sequentially?
		var parallelizeAssemblies = project.Assemblies.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);

		if (needsXml)
			assembliesElement = new XElement("assemblies");

		var originalWorkingFolder = Directory.GetCurrentDirectory();

		if (parallelizeAssemblies)
		{
			var tasks = project.Assemblies.Select(
				assembly => Task.Run(
					() => RunProjectAssembly(
						assembly,
						needsXml,
						reporterMessageHandler
					).AsTask()
				)
			);

			var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
			foreach (var assemblyElement in results.WhereNotNull())
				assembliesElement?.Add(assemblyElement);
		}
		else
		{
			foreach (var assembly in project.Assemblies)
			{
				var assemblyElement = await RunProjectAssembly(
					assembly,
					needsXml,
					reporterMessageHandler
				);

				if (assemblyElement != null)
					assembliesElement?.Add(assemblyElement);
			}
		}

		clockTime.Stop();

		if (assembliesElement != null)
			assembliesElement.Add(new XAttribute("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

		if (completionMessages.Count > 0)
		{
			var summaries = new TestExecutionSummaries { ElapsedClockTime = clockTime.Elapsed };
			foreach (var completionMessage in completionMessages.OrderBy(kvp => kvp.Key))
				summaries.Add(completionMessage.Key, completionMessage.Value);
			reporterMessageHandler.OnMessage(summaries);
		}

		Directory.SetCurrentDirectory(originalWorkingFolder);

		if (assembliesElement != null)
			xmlTransformers.ForEach(transformer => transformer(assembliesElement));

		return failed ? 1 : completionMessages.Values.Sum(summary => summary.Failed + summary.Errors);
	}

	async ValueTask<XElement?> RunProjectAssembly(
		XunitProjectAssembly assembly,
		bool needsXml,
		_IMessageSink reporterMessageHandler)
	{
		if (cancel)
			return null;

		var assemblyElement = needsXml ? new XElement("assembly") : null;

		try
		{
			var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);

			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);

			var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFileName);
			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.ForDiagnostics(consoleLock, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault, noColor);
			var internalDiagnosticsMessageSink = ConsoleDiagnosticMessageSink.ForInternalDiagnostics(consoleLock, assemblyDisplayName, assembly.Configuration.InternalDiagnosticMessagesOrDefault, noColor);
			var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
			var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			using var _ = AssemblyHelper.SubscribeResolveForAssembly(assemblyFileName, internalDiagnosticsMessageSink);
			await using var controller = XunitFrontController.ForDiscoveryAndExecution(assembly, diagnosticMessageSink: diagnosticMessageSink);

			var executionStarting = new TestAssemblyExecutionStarting
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions
			};
			reporterMessageHandler.OnMessage(executionStarting);

			IExecutionSink resultsSink = new DelegatingExecutionSummarySink(reporterMessageHandler, () => cancel, (summary, _) => completionMessages.TryAdd(controller.TestAssemblyUniqueID, summary));
			if (assemblyElement != null)
				resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
			if (longRunningSeconds > 0)
				resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink);
			if (assembly.Configuration.FailSkipsOrDefault)
				resultsSink = new DelegatingFailSkipSink(resultsSink);

			using (resultsSink)
			{
				var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, assembly.Configuration.Filters);
				controller.FindAndRun(resultsSink, settings);
				resultsSink.Finished.WaitOne();

				var executionFinished = new TestAssemblyExecutionFinished
				{
					Assembly = assembly,
					ExecutionOptions = executionOptions,
					ExecutionSummary = resultsSink.ExecutionSummary
				};
				reporterMessageHandler.OnMessage(executionFinished);

				if (assembly.Configuration.StopOnFailOrDefault && resultsSink.ExecutionSummary.Failed != 0)
				{
					Console.WriteLine("Canceling due to test failure...");
					cancel = true;
				}
			}
		}
		catch (Exception ex)
		{
			failed = true;

			var e = ex;
			while (e != null)
			{
				Console.WriteLine($"{e.GetType().FullName}: {e.Message}");

				if (assembly.Configuration.InternalDiagnosticMessagesOrDefault)
					Console.WriteLine(e.StackTrace);

				e = e.InnerException;
			}
		}

		return assemblyElement;
	}
}
