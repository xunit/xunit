using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary>
/// This class is the entry point for the in-process console-based runner used for
/// xUnit.net v3 test projects.
/// </summary>
public class ConsoleRunner
{
	readonly string[] args;
	bool automated;
	volatile bool cancel;
	readonly object consoleLock;
	bool executed;
	bool failed;
	IRunnerLogger? logger;
	IReadOnlyList<IRunnerReporter>? runnerReporters;
	bool started;
	readonly Assembly testAssembly;
	readonly TestExecutionSummaries testExecutionSummaries = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="ConsoleRunner"/> class.
	/// </summary>
	/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
	/// <param name="testAssembly">The (optional) assembly to test; defaults to <see cref="Assembly.GetEntryAssembly"/>.</param>
	/// <param name="runnerReporters">The (optional) list of runner reporters.</param>
	/// <param name="consoleLock">The (optional) lock used around all console output to ensure there are no write collisions.</param>
	public ConsoleRunner(
		string[] args,
		Assembly? testAssembly = null,
		IEnumerable<IRunnerReporter>? runnerReporters = null,
		object? consoleLock = null)
	{
		this.args = Guard.ArgumentNotNull(args);
		this.testAssembly = Guard.ArgumentNotNull("testAssembly was null, and Assembly.GetEntryAssembly() returned null; you should pass a non-null value for testAssembly", testAssembly ?? Assembly.GetEntryAssembly(), nameof(testAssembly));
		this.consoleLock = consoleLock ?? new object();
		this.runnerReporters = runnerReporters.CastOrToReadOnlyList();
	}

	/// <summary>
	/// The entry point to begin running tests.
	/// </summary>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	public async ValueTask<int> EntryPoint()
	{
		if (executed)
			throw new InvalidOperationException("The EntryPoint method can only be called once.");

		executed = true;

		SetOutputEncoding();

		var globalInternalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var commandLine = new CommandLine(testAssembly, args, runnerReporters);

			if (commandLine.HelpRequested)
			{
				PrintHeader();

				Console.WriteLine("Copyright (C) .NET Foundation.");
				Console.WriteLine();

				if (commandLine.ParseWarnings.Count > 0)
				{
					foreach (var warning in commandLine.ParseWarnings)
						Console.WriteLine("Warning: {0}", warning);

					Console.WriteLine();
				}

				Console.WriteLine("usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");

				commandLine.PrintUsage();
				return 2;
			}

			// We pick up the -automated flag early, because Parse() can throw and we want to use automated output
			// to report any command line parsing problems.
			automated = commandLine.AutomatedRequested;
			if (automated)
				noColor = true;

			var project = commandLine.Parse();
			var useAnsiColor = project.Configuration.UseAnsiColorOrDefault;
			if (useAnsiColor)
				ConsoleHelper.UseAnsiColor();

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Console.CancelKeyPress += (sender, e) =>
			{
				if (started && !cancel)
				{
					if (automated)
						Console.WriteLine(new _DiagnosticMessage("Cancellation request received").ToJson());
					else
						Console.WriteLine("Canceling... (Press Ctrl+C again to terminate)");

					cancel = true;
					e.Cancel = true;
				}
			};

			if (project.Configuration.PauseOrDefault)
			{
				if (!automated)
					Console.Write("Press any key to start execution...");

				Console.ReadKey(true);

				if (!automated)
					Console.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			var globalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.DiagnosticMessagesOrDefault);
			globalInternalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);

			if (!automated)
				noColor = project.Configuration.NoColorOrDefault;

			logger = new ConsoleRunnerLogger(!noColor, useAnsiColor, consoleLock);

			_IMessageSink? globalDiagnosticMessageSink =
				automated
					? new AutomatedDiagnosticMessageSink(consoleLock)
					: ConsoleDiagnosticMessageSink.TryCreate(consoleLock, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);

			var reporter = automated ? new JsonReporter() : project.RunnerReporter;
			var reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticMessageSink);

			if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
				PrintHeader();

			foreach (string warning in commandLine.ParseWarnings)
				if (automated)
					Console.WriteLine(new _DiagnosticMessage("warning: " + warning).ToJson());
				else
					logger.LogWarning(warning);

			var failCount = 0;

			if (project.Configuration.WaitForDebuggerOrDefault)
			{
				if (!automated)
					Console.WriteLine("Waiting for debugger to be attached... (press Ctrl+C to abort)");

				while (true)
				{
					if (Debugger.IsAttached)
						break;

					await Task.Delay(10);
				}
			}

			started = true;

			if (project.Configuration.List is not null)
				await ListProject(project, automated);
			else
				failCount = await RunProject(project, reporterMessageHandler);

			if (cancel)
				return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C

			if (project.Configuration.WaitOrDefault)
			{
				if (!automated)
				{
					Console.WriteLine();
					Console.Write("Press any key to continue...");
				}

				Console.ReadKey();

				if (!automated)
					Console.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				ConsoleHelper.SetForegroundColor(ConsoleColor.Red);

			if (automated)
				Console.WriteLine(new _DiagnosticMessage("error: " + ex.Message).ToJson());
			else
			{
				Console.WriteLine("error: {0}", ex.Message);

				if (globalInternalDiagnosticMessages)
				{
					if (!noColor)
						ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

					Console.WriteLine(ex.StackTrace);
				}
			}

			return ex is ArgumentException ? 3 : 4;
		}
		finally
		{
			if (!noColor)
				ConsoleHelper.ResetColor();
		}
	}

	async ValueTask ListProject(
		XunitProject project,
		bool automated)
	{
		var (listOption, listFormat) = project.Configuration.List!.Value;
		if (automated)
			listFormat = ListFormat.Json;

		var testCasesByAssembly = new Dictionary<string, List<_ITestCase>>();

		foreach (var assembly in project.Assemblies)
		{
			var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);

			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery options with command line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);

			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleLock, noColor, diagnosticMessages, internalDiagnosticMessages);

			TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages, internalDiagnosticMessages);

			var assemblyInfo = new ReflectionAssemblyInfo(testAssembly);

#pragma warning disable CA2007 // Cannot use ConfigureAwait here because it changes the type of disposalTracker
			await using var disposalTracker = new DisposalTracker();
#pragma warning restore CA2007
			var testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
			disposalTracker.Add(testFramework);

			// Discover & filter the tests
			var testCases = new List<_ITestCase>();
			var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
			await testDiscoverer.Find(testCase => { testCases.Add(testCase); return new(!cancel); }, discoveryOptions);

			var testCasesDiscovered = testCases.Count;
			var filteredTestCases = testCases.Where(assembly.Configuration.Filters.Filter).ToList();

			testCasesByAssembly.Add(assemblyFileName, filteredTestCases);
		}

		ConsoleProjectLister.List(testCasesByAssembly, listOption, listFormat);
	}

	void OnUnhandledException(
		object sender,
		UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			if (automated)
				Console.WriteLine(_ErrorMessage.FromException(ex).ToJson());
			else
				Console.WriteLine(ex.ToString());
		}
		else
		{
			if (automated)
				Console.WriteLine(new _DiagnosticMessage("Error of unknown type thrown in application domain").ToJson());
			else
				Console.WriteLine("Error of unknown type thrown in application domain");
		}

		Environment.Exit(1);
	}

	static void PrintHeader() =>
		Console.WriteLine(
			"xUnit.net v3 In-Process Runner v{0} ({1}-bit {2})",
			ThisAssembly.AssemblyInformationalVersion,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription
		);

	/// <summary>
	/// Creates a new <see cref="ConsoleRunner"/> instance and runs it via <see cref="EntryPoint"/>.
	/// </summary>
	/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
	/// <param name="testAssembly">The (optional) assembly to test; defaults to <see cref="Assembly.GetEntryAssembly"/>.</param>
	/// <param name="runnerReporters">The (optional) list of runner reporters.</param>
	/// <param name="consoleLock">The (optional) lock used around all console output to ensure there are no write collisions.</param>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	public static ValueTask<int> Run(
		string[] args,
		Assembly? testAssembly = null,
		IEnumerable<IRunnerReporter>? runnerReporters = null,
		object? consoleLock = null) =>
			new ConsoleRunner(args, testAssembly, runnerReporters, consoleLock).EntryPoint();

	async ValueTask<int> RunProject(
		XunitProject project,
		_IMessageSink reporterMessageHandler)
	{
		XElement? assembliesElement = null;
		var clockTime = Stopwatch.StartNew();
		var xmlTransformers = TransformFactory.GetXmlTransformers(project);
		var needsXml = xmlTransformers.Count > 0;

		if (needsXml)
			assembliesElement = TransformFactory.CreateAssembliesElement();

		var originalWorkingFolder = Directory.GetCurrentDirectory();

		var assembly = project.Assemblies.Single();
		var assemblyElement = await RunProjectAssembly(
			assembly,
			needsXml,
			reporterMessageHandler
		);

		if (assemblyElement is not null)
			assembliesElement?.Add(assemblyElement);

		clockTime.Stop();

		testExecutionSummaries.ElapsedClockTime = clockTime.Elapsed;
		reporterMessageHandler.OnMessage(testExecutionSummaries);

		Directory.SetCurrentDirectory(originalWorkingFolder);

		if (assembliesElement is not null)
		{
			TransformFactory.FinishAssembliesElement(assembliesElement);
			xmlTransformers.ForEach(transformer => transformer(assembliesElement));
		}

		return failed ? 1 : testExecutionSummaries.SummariesByAssemblyUniqueID.Sum(s => s.Summary.Failed + s.Summary.Errors);
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
			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);

			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleLock, noColor, diagnosticMessages, internalDiagnosticMessages);
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages, internalDiagnosticMessages);

			var assemblyInfo = new ReflectionAssemblyInfo(testAssembly);

#pragma warning disable CA2007 // Cannot use ConfigureAwait here because it changes the type of disposalTracker
			await using var disposalTracker = new DisposalTracker();
#pragma warning restore CA2007
			var testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
			disposalTracker.Add(testFramework);

			var frontController = new InProcessFrontController(testFramework, assemblyInfo, assembly.ConfigFileName);

			var sinkOptions = new ExecutionSinkOptions
			{
				AssemblyElement = assemblyElement,
				CancelThunk = () => cancel,
				DiagnosticMessageSink = diagnosticMessageSink,
				FailSkips = assembly.Configuration.FailSkipsOrDefault,
				FailWarn = assembly.Configuration.FailTestsWithWarningsOrDefault,
				LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
			};

			using var resultsSink = new ExecutionSink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, shadowCopy: false, reporterMessageHandler, sinkOptions);
			var testCases =
				assembly
					.TestCasesToRun
					.Select(s => SerializationHelper.Deserialize(s) as _ITestCase)
					.WhereNotNull()
					.ToArray();

			if (testCases.Length != 0)
				await frontController.Run(resultsSink, executionOptions, testCases);
			else
				await frontController.FindAndRun(resultsSink, discoveryOptions, executionOptions, assembly.Configuration.Filters.Filter);

			testExecutionSummaries.Add(frontController.TestAssemblyUniqueID, resultsSink.ExecutionSummary);

			if (resultsSink.ExecutionSummary.Failed != 0 && executionOptions.GetStopOnTestFailOrDefault())
			{
				if (automated)
					Console.WriteLine(new _DiagnosticMessage("Canceling due to test failure").ToJson());
				else
					Console.WriteLine("Canceling due to test failure...");

				cancel = true;
			}
		}
		catch (Exception ex)
		{
			failed = true;

			var e = ex;
			while (e is not null)
			{
				if (automated)
					Console.WriteLine(_ErrorMessage.FromException(e).ToJson());
				else
				{
					Console.WriteLine("{0}: {1}", e.GetType().FullName, e.Message);

					if (assembly.Configuration.InternalDiagnosticMessagesOrDefault)
						Console.WriteLine(e.StackTrace);
				}

				e = e.InnerException;
			}
		}

		return assemblyElement;
	}

	/// <summary>
	/// Override this function to change the default output encoding for the system console.
	/// The default is set to <see cref="Encoding.UTF8"/> to support our usage of Unicode
	/// characters in output (for example, the up and down arrows printed for pointers with
	/// mismatched assertion values).
	/// </summary>
	protected virtual void SetOutputEncoding() =>
		Console.OutputEncoding = Encoding.UTF8;
}
