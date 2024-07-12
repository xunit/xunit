#pragma warning disable CA1849  // We don't want to use the async versions wrapping Console.WriteLine because they're less featureful

using System;
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

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary>
/// This class is the entry point for the in-process console-based runner used for
/// xUnit.net v3 test projects.
/// </summary>
/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
public class ConsoleRunner(string[] args)
{
	readonly string[] args = Guard.ArgumentNotNull(args);
	bool automated;
	volatile bool cancel;
	TextWriter consoleWriter = default!;
	bool executed;
	bool failed;
	IRunnerLogger? logger;
	ITestPipelineStartup? pipelineStartup;
	bool started;
	readonly Assembly testAssembly = Guard.NotNull("Assembly.GetEntryAssembly() returned null", Assembly.GetEntryAssembly());
	readonly TestExecutionSummaries testExecutionSummaries = new();

	/// <summary>
	/// The entry point to begin running tests.
	/// </summary>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	public async Task<int> EntryPoint()
	{
		if (executed)
			throw new InvalidOperationException("The EntryPoint method can only be called once.");

		executed = true;

		SetOutputEncoding();
		consoleWriter = Console.Out;
		ConsoleHelper.ConsoleWriter = consoleWriter;

		var globalInternalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var commandLine = new CommandLine(consoleWriter, testAssembly, args);

			if (commandLine.HelpRequested)
			{
				PrintHeader();

				consoleWriter.WriteLine("Copyright (C) .NET Foundation.");
				consoleWriter.WriteLine();

				if (commandLine.ParseWarnings.Count > 0)
				{
					foreach (var warning in commandLine.ParseWarnings)
						consoleWriter.WriteLine("Warning: {0}", warning);

					consoleWriter.WriteLine();
				}

				consoleWriter.WriteLine("usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");

				commandLine.PrintUsage();
				return 2;
			}

			// We pick up the -automated flag early, because Parse() can throw and we want to use automated output
			// to report any command line parsing problems.
			automated = commandLine.AutomatedRequested;
			if (automated)
			{
				Console.SetOut(TextWriter.Null);
				noColor = true;
			}

			var project = commandLine.Parse();
			var useAnsiColor = project.Configuration.UseAnsiColorOrDefault;
			if (useAnsiColor)
				ConsoleHelper.UseAnsiColor();

			if (project.Configuration.AssemblyInfoOrDefault)
			{
				noColor = true;
				PrintAssemblyInfo();
				return 0;
			}

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Console.CancelKeyPress += (sender, e) =>
			{
				if (started && !cancel)
				{
					if (automated)
						consoleWriter.WriteLine(new DiagnosticMessage("Cancellation request received").ToJson());
					else
						consoleWriter.WriteLine("Cancelling... (Press Ctrl+C again to terminate)");

					cancel = true;
					e.Cancel = true;
				}
			};

			if (project.Configuration.PauseOrDefault)
			{
				if (!automated)
					consoleWriter.Write("Press any key to start execution...");

				Console.ReadKey(true);

				if (!automated)
					consoleWriter.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			var globalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.DiagnosticMessagesOrDefault);
			globalInternalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);

			if (!automated)
				noColor = project.Configuration.NoColorOrDefault;

			logger = new ConsoleRunnerLogger(!noColor, useAnsiColor, consoleWriter);

			IMessageSink? globalDiagnosticMessageSink =
				automated
					? new AutomatedDiagnosticMessageSink(consoleWriter)
					: ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);

			var pipelineStartupAttributes = testAssembly.GetMatchingCustomAttributes(typeof(ITestPipelineStartupAttribute));
			if (pipelineStartupAttributes.Count > 1)
				throw new TestPipelineException("More than one pipeline startup attribute was specified: " + pipelineStartupAttributes.Select(a => a.GetType()).ToCommaSeparatedList());

			if (pipelineStartupAttributes.FirstOrDefault() is ITestPipelineStartupAttribute pipelineStartupAttribute)
			{
				var pipelineStartupType = pipelineStartupAttribute.TestPipelineStartupType;
				if (!typeof(ITestPipelineStartup).IsAssignableFrom(pipelineStartupType))
					throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "Pipeline startup type '{0}' does not implement '{1}'", pipelineStartupType.SafeName(), typeof(ITestPipelineStartup).SafeName()));

				try
				{
					pipelineStartup = Activator.CreateInstance(pipelineStartupType) as ITestPipelineStartup;
				}
				catch (Exception ex)
				{
					throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "Pipeline startup type '{0}' threw during construction", pipelineStartupType.SafeName()), ex);
				}

				if (pipelineStartup is null)
					throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "Pipeline startup type '{0}' does not implement '{1}'", pipelineStartupType.SafeName(), typeof(ITestPipelineStartup).SafeName()));

				IMessageSink? pipelineMessageSink =
					automated
						? new AutomatedDiagnosticMessageSink(consoleWriter)
						: ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, globalDiagnosticMessages, indent: false, assemblyDisplayName: pipelineStartupType.SafeName());

				await pipelineStartup.StartAsync(pipelineMessageSink ?? NullMessageSink.Instance);
			}

			var failCount = 0;

			try
			{
				var reporter = automated ? new JsonReporter() : project.RunnerReporter;
				var reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticMessageSink);

				if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
					PrintHeader();

				foreach (string warning in commandLine.ParseWarnings)
					if (automated)
						consoleWriter.WriteLine(new DiagnosticMessage("warning: " + warning).ToJson());
					else
						logger.LogWarning(warning);

				if (project.Configuration.WaitForDebuggerOrDefault)
				{
					if (!automated)
						consoleWriter.WriteLine("Waiting for debugger to be attached... (press Ctrl+C to abort)");

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
			}
			finally
			{
				if (pipelineStartup is not null)
					await pipelineStartup.StopAsync();
			}

			if (project.Configuration.WaitOrDefault)
			{
				if (!automated)
				{
					consoleWriter.WriteLine();
					consoleWriter.Write("Press any key to continue...");
				}

				Console.ReadKey();

				if (!automated)
					consoleWriter.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				ConsoleHelper.SetForegroundColor(ConsoleColor.Red);

			if (automated)
				consoleWriter.WriteLine(new DiagnosticMessage("error: " + ex.Message).ToJson());
			else
			{
				consoleWriter.WriteLine("error: {0}", ex.Message);

				if (globalInternalDiagnosticMessages)
				{
					if (!noColor)
						ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

					consoleWriter.WriteLine(ex.StackTrace);
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

		var testCasesByAssembly = new Dictionary<string, List<ITestCase>>();

		foreach (var assembly in project.Assemblies)
		{
			var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);

			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery options with command line overrides
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);

			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, diagnosticMessages, internalDiagnosticMessages);

			TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages, internalDiagnosticMessages);

			await using var disposalTracker = new DisposalTracker();
			var testFramework = ExtensibilityPointFactory.GetTestFramework(testAssembly);
			disposalTracker.Add(testFramework);

			if (pipelineStartup is not null)
				testFramework.SetTestPipelineStartup(pipelineStartup);

			// Discover & filter the tests
			var testCases = new List<ITestCase>();
			var testDiscoverer = testFramework.GetDiscoverer(testAssembly);
			var types =
				assembly.Configuration.Filters.IncludedClasses.Count == 0 || assembly.Assembly is null
					? null
					: assembly.Configuration.Filters.IncludedClasses.Select(assembly.Assembly.GetType).WhereNotNull().ToArray();

			await testDiscoverer.Find(testCase => { testCases.Add(testCase); return new(!cancel); }, discoveryOptions, types);

			var testCasesDiscovered = testCases.Count;
			var filteredTestCases = testCases.Where(assembly.Configuration.Filters.Filter).ToList();

			testCasesByAssembly.Add(assemblyFileName, filteredTestCases);
		}

		if (listOption != ListOption.Discovery)
			ConsoleProjectLister.List(consoleWriter, testCasesByAssembly, listOption, listFormat);
		else
			foreach (var testCase in testCasesByAssembly.SelectMany(kvp => kvp.Value))
				consoleWriter.WriteLine(testCase.ToTestCaseDiscovered().ToJson());
	}

	void OnUnhandledException(
		object sender,
		UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			if (automated)
				consoleWriter.WriteLine(ErrorMessage.FromException(ex).ToJson());
			else
				consoleWriter.WriteLine(ex.ToString());
		}
		else
		{
			if (automated)
				consoleWriter.WriteLine(new DiagnosticMessage("Error of unknown type thrown in application domain").ToJson());
			else
				consoleWriter.WriteLine("Error of unknown type thrown in application domain");
		}

		Environment.Exit(1);
	}

	void PrintAssemblyInfo()
	{
		var testFramework = ExtensibilityPointFactory.GetTestFramework(testAssembly);

		var buffer = new StringBuilder();
		using (var serializer = new JsonObjectSerializer(buffer))
		{
			serializer.Serialize("arch-os", RuntimeInformation.OSArchitecture);
			serializer.Serialize("arch-process", RuntimeInformation.ProcessArchitecture);
			// Technically these next two are the versions of xunit.v3.runner.inproc.console and not xunit.v3.core; however,
			// since they're all compiled and versioned together, we'll take the path of least resistance.
			serializer.Serialize("core-framework", ThisAssembly.AssemblyVersion);
			serializer.Serialize("core-framework-informational", ThisAssembly.AssemblyInformationalVersion);
			serializer.Serialize("pointer-size", IntPtr.Size * 8);
			serializer.Serialize("runtime-framework", RuntimeInformation.FrameworkDescription);
			serializer.Serialize("target-framework", testAssembly.GetTargetFramework());
			serializer.Serialize("test-framework", testFramework.TestFrameworkDisplayName);
		}

		consoleWriter.WriteLine(buffer.ToString());
	}

	void PrintHeader() =>
		consoleWriter.WriteLine(
			"xUnit.net v3 In-Process Runner v{0} ({1}-bit {2})",
			ThisAssembly.AssemblyInformationalVersion,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription
		);

	/// <summary>
	/// Creates a new <see cref="ConsoleRunner"/> instance and runs it via <see cref="EntryPoint"/>.
	/// </summary>
	/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	// Note: This returns Task instead of ValueTask, because it's called from the injected entry point, and we don't want to
	// assume that the global entry point can use an async Main method (for acceptance testing purposes).
	public static Task<int> Run(string[] args) =>
		new ConsoleRunner(args).EntryPoint();

	async ValueTask<int> RunProject(
		XunitProject project,
		IMessageSink reporterMessageHandler)
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
		IMessageSink reporterMessageHandler)
	{
		if (cancel)
			return null;

		var assemblyElement = needsXml ? new XElement("assembly") : null;

		try
		{
			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);

			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleWriter, noColor, diagnosticMessages, internalDiagnosticMessages);
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages, internalDiagnosticMessages);

			await using var disposalTracker = new DisposalTracker();
			var testFramework = ExtensibilityPointFactory.GetTestFramework(testAssembly);
			disposalTracker.Add(testFramework);

			var frontController = new InProcessFrontController(testFramework, testAssembly, assembly.ConfigFileName);

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
					.Select(s => SerializationHelper.Deserialize(s) as ITestCase)
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
					consoleWriter.WriteLine(new DiagnosticMessage("Cancelling due to test failure").ToJson());
				else
					consoleWriter.WriteLine("Cancelling due to test failure...");

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
					consoleWriter.WriteLine(ErrorMessage.FromException(e).ToJson());
				else
				{
					consoleWriter.WriteLine("{0}: {1}", e.GetType().SafeName(), e.Message);

					if (assembly.Configuration.InternalDiagnosticMessagesOrDefault)
						consoleWriter.WriteLine(e.StackTrace);
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
