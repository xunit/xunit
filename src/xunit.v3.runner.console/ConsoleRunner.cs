#pragma warning disable CA1849  // We don't want to use the async versions wrapping Console.WriteLine because they're less featureful

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.SystemConsole;

sealed class ConsoleRunner
{
	readonly string[] args;
	volatile bool cancel;
	readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new();
	TextWriter consoleWriter = default!;
	bool failed;
	IRunnerLogger? logger;

	public ConsoleRunner(string[] args)
	{
		this.args = Guard.ArgumentNotNull(args);
	}

	public async ValueTask<int> EntryPoint()
	{
		// Stashing and manipulating the console writer here mirrors the in-process runner, which only overrides
		// Console.Out when in automated mode. In effort to keep as much code here duplicated as possible (for
		// future base-class extraction), we do this even though it's not strictly speaking necessary.
		Console.OutputEncoding = Encoding.UTF8;
		consoleWriter = Console.Out;
		ConsoleHelper.ConsoleWriter = consoleWriter;

		var globalInternalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var runnerFolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			var commandLine = new CommandLine(consoleWriter, runnerFolder, args);

			if (args.Length == 0 || commandLine.HelpRequested)
			{
				PrintHeader();

				var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

				consoleWriter.WriteLine("Copyright (C) .NET Foundation.");
				consoleWriter.WriteLine();

				if (commandLine.ParseWarnings.Count > 0)
				{
					foreach (var warning in commandLine.ParseWarnings)
						consoleWriter.WriteLine("Warning: {0}", warning);

					consoleWriter.WriteLine();
				}

				consoleWriter.WriteLine("usage: {0} <assemblyFile>[:seed] [configFile] [assemblyFile[:seed] [configFile]...] [options] [reporter] [resultFormat filename [...]]", executableName);
				consoleWriter.WriteLine();
				consoleWriter.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
				consoleWriter.WriteLine("      XML is supported for v1 and v2 only, on .NET Framework only");
				consoleWriter.WriteLine("      JSON is supported for v2 and later, on all supported platforms");

				commandLine.PrintUsage();

				return 2;
			}

			var project = commandLine.Parse();
			var useAnsiColor = project.Configuration.UseAnsiColorOrDefault;
			if (useAnsiColor)
				ConsoleHelper.UseAnsiColor();
			if (project.Assemblies.Count == 0)
				throw new ArgumentException("must specify at least one assembly");

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Console.CancelKeyPress += (sender, e) =>
			{
				if (!cancel)
				{
					consoleWriter.WriteLine("Canceling... (Press Ctrl+C again to terminate)");
					cancel = true;
					e.Cancel = true;
				}
			};

			if (project.Configuration.PauseOrDefault)
			{
				consoleWriter.Write("Press any key to start execution...");
				Console.ReadKey(true);
				consoleWriter.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			var globalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.DiagnosticMessagesOrDefault);
			globalInternalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);
			noColor = project.Configuration.NoColorOrDefault;
			logger = new ConsoleRunnerLogger(!noColor, useAnsiColor, consoleWriter);
			var globalDiagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);
			var reporter = project.RunnerReporter;
			var reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticMessageSink);

			if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
				PrintHeader();

			foreach (string warning in commandLine.ParseWarnings)
				logger.LogWarning(warning);

			var failCount = 0;

			if (project.Configuration.List is not null)
				await ListProject(project);
			else
				failCount = await RunProject(project, reporterMessageHandler);

			if (cancel)
				return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C

			if (project.Configuration.WaitOrDefault)
			{
				consoleWriter.WriteLine();
				consoleWriter.Write("Press any key to continue...");
				Console.ReadKey();
				consoleWriter.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				ConsoleHelper.SetForegroundColor(ConsoleColor.Red);

			consoleWriter.WriteLine("error: {0}", ex.Message);

			if (globalInternalDiagnosticMessages)
			{
				if (!noColor)
					ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

				consoleWriter.WriteLine(ex.StackTrace);
			}

			return ex is ArgumentException ? 3 : 4;
		}
		finally
		{
			if (!noColor)
				ConsoleHelper.ResetColor();
		}
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
			await using var controller =
				XunitFrontController.ForDiscoveryAndExecution(assembly)
					?? throw new ArgumentException("not an xUnit.net test assembly: {0}", assemblyFileName);

			using var discoverySink = new TestDiscoverySink(() => cancel);

			var settings = new FrontControllerFindSettings(discoveryOptions, assembly.Configuration.Filters);
			controller.Find(discoverySink, settings);
			discoverySink.Finished.WaitOne();

			testCasesByAssembly.Add(assemblyFileName, discoverySink.TestCases);
		}

		ConsoleProjectLister.List(consoleWriter, testCasesByAssembly, listOption, listFormat);
	}

	void OnUnhandledException(
		object sender,
		UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
			consoleWriter.WriteLine(ex.ToString());
		else
			consoleWriter.WriteLine("Error of unknown type thrown in application domain");

		Environment.Exit(1);
	}

	void PrintHeader()
	{
#if NET472
		var buildTarget = "net472" +
#elif NET48
		var buildTarget = "net48" +
#elif NET481
		var buildTarget = "net481" +
#else
#error Unknown target framework
#endif

#if BUILD_X86
		"/x86";
#else
		"/AnyCPU";
#endif

		consoleWriter.WriteLine(
			"xUnit.net v3 Console Runner v{0} [{1}] ({2}-bit {3})",
			ThisAssembly.AssemblyInformationalVersion,
			buildTarget,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription
		);
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
			assembliesElement = TransformFactory.CreateAssembliesElement();

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

				if (assemblyElement is not null)
					assembliesElement?.Add(assemblyElement);
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

		Directory.SetCurrentDirectory(originalWorkingFolder);

		if (assembliesElement is not null)
		{
			TransformFactory.FinishAssembliesElement(assembliesElement);
			xmlTransformers.ForEach(transformer => transformer(assembliesElement));
		}

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
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, diagnosticMessages, internalDiagnosticMessages, assemblyDisplayName);
			var appDomainSupport = assembly.Configuration.AppDomainOrDefault;
			var shadowCopy = assembly.Configuration.ShadowCopyOrDefault;
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			using var _ = AssemblyHelper.SubscribeResolveForAssembly(assemblyFileName, diagnosticMessageSink);
			await using var controller =
				XunitFrontController.ForDiscoveryAndExecution(assembly, diagnosticMessageSink: diagnosticMessageSink)
					?? throw new ArgumentException("not an xUnit.net test assembly: {0}", assemblyFileName);

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
				CancelThunk = () => cancel,
				DiagnosticMessageSink = diagnosticMessageSink,
				FailSkips = assembly.Configuration.FailSkipsOrDefault,
				FailWarn = assembly.Configuration.FailTestsWithWarningsOrDefault,
				FinishedCallback = summary => completionMessages.TryAdd(controller.TestAssemblyUniqueID, summary),
				LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
			};

			using var resultsSink = new ExecutionSink(assembly, discoveryOptions, executionOptions, appDomain, shadowCopy, reporterMessageHandler, sinkOptions);
			var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, assembly.Configuration.Filters);
			controller.FindAndRun(resultsSink, settings);
			resultsSink.Finished.WaitOne();

			if (resultsSink.ExecutionSummary.Failed != 0 && executionOptions.GetStopOnTestFailOrDefault())
			{
				consoleWriter.WriteLine("Canceling due to test failure...");
				cancel = true;
			}
		}
		catch (Exception ex)
		{
			failed = true;

			var e = ex;
			while (e is not null)
			{
				consoleWriter.WriteLine("{0}: {1}", e.GetType().FullName, e.Message);

				if (assembly.Configuration.InternalDiagnosticMessagesOrDefault)
					consoleWriter.WriteLine(e.StackTrace);

				e = e.InnerException;
			}
		}

		return assemblyElement;
	}
}
