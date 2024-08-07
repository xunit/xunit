using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;
using DiagnosticMessage = Xunit.Runner.Common.DiagnosticMessage;
using ErrorMessage = Xunit.Runner.Common.ErrorMessage;

namespace Xunit.Runner.InProc.SystemConsole;

/// <summary>
/// This class is the entry point for the in-process console-based runner used for
/// xUnit.net v3 test projects.
/// </summary>
/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
/// <param name="testAssembly">The optional test assembly; if <c>null</c>, <see cref="Assembly.GetEntryAssembly"/> is used
/// to find the current assembly as the test assembly</param>
// The signature of the constructor is known to Xunit3, so do not change it without also changing
// the code that invokes it dynamically.
public class ConsoleRunner(
	string[] args,
	Assembly? testAssembly = null)
{
	readonly string[] args = Guard.ArgumentNotNull(args);
	bool automated;
	volatile bool cancel;
	ConsoleHelper consoleHelper = default!;
	bool executed;
	IRunnerLogger? logger;
	ITestPipelineStartup? pipelineStartup;
	bool started;
	readonly Assembly testAssembly = Guard.NotNull("Assembly.GetEntryAssembly() returned null", testAssembly ?? Assembly.GetEntryAssembly());

	/// <summary>
	/// Attempt to cancel the console runner execution.
	/// </summary>
	// The signature of this method is known to Xunit3, so do not change it without also changing
	// the code that invokes it dynamically.
	public void Cancel() =>
		cancel = true;

	/// <summary>
	/// The entry point to begin running tests.
	/// </summary>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	// The signature of this method is known to Xunit3, so do not change it without also changing
	// the code that invokes it dynamically.
	public async Task<int> EntryPoint(TextWriter? consoleOverride = null)
	{
		if (executed)
			throw new InvalidOperationException("The EntryPoint method can only be called once.");

		executed = true;

		if (consoleOverride is null)
			SetOutputEncoding();

		consoleHelper = new(consoleOverride ?? Console.Out);

		var globalInternalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var commandLine = new CommandLine(consoleHelper, testAssembly, args);

			if (commandLine.HelpRequested)
			{
				ProjectAssemblyRunner.PrintHeader(consoleHelper);

				consoleHelper.WriteLine("Copyright (C) .NET Foundation.");
				consoleHelper.WriteLine();

				if (commandLine.ParseWarnings.Count > 0)
				{
					foreach (var warning in commandLine.ParseWarnings)
						consoleHelper.WriteLine("Warning: {0}", warning);

					consoleHelper.WriteLine();
				}

				consoleHelper.WriteLine("usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");

				commandLine.PrintUsage();
				return 2;
			}

			// We pick up the -automated flag early, because Parse() can throw and we want to use automated output
			// to report any command line parsing problems.
			automated = commandLine.AutomatedRequested;
			if (automated)
			{
				if (consoleOverride is null)
					Console.SetOut(TextWriter.Null);

				noColor = true;
			}

			var projectAssembly = commandLine.Parse();
			var project = projectAssembly.Project;
			var useAnsiColor = project.Configuration.UseAnsiColorOrDefault;
			if (useAnsiColor)
				consoleHelper.UseAnsiColor();

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
						consoleHelper.WriteLine(new DiagnosticMessage("Cancellation request received").ToJson());
					else
						consoleHelper.WriteLine("Cancelling... (Press Ctrl+C again to terminate)");

					cancel = true;
					e.Cancel = true;
				}
			};

			if (project.Configuration.PauseOrDefault)
			{
				if (!automated)
					consoleHelper.Write("Press any key to start execution...");

				Console.ReadKey(true);

				if (!automated)
					consoleHelper.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			var globalDiagnosticMessages = projectAssembly.Configuration.DiagnosticMessagesOrDefault;
			globalInternalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;

			if (!automated)
				noColor = project.Configuration.NoColorOrDefault;

			logger = new ConsoleRunnerLogger(!noColor, useAnsiColor, consoleHelper);

			IMessageSink? globalDiagnosticMessageSink =
				automated
					? new AutomatedDiagnosticMessageSink(consoleHelper)
					: ConsoleDiagnosticMessageSink.TryCreate(consoleHelper, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);

			pipelineStartup = await ProjectAssemblyRunner.InvokePipelineStartup(testAssembly, consoleHelper, automated, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);

			var failCount = 0;

			try
			{
				var reporter = automated ? new JsonReporter() : project.RunnerReporter;
				var reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticMessageSink);

				if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
					ProjectAssemblyRunner.PrintHeader(consoleHelper);

				foreach (string warning in commandLine.ParseWarnings)
					if (automated)
						consoleHelper.WriteLine(new DiagnosticMessage("warning: " + warning).ToJson());
					else
						logger.LogWarning(warning);

				var projectRunner = new ProjectAssemblyRunner(testAssembly, consoleHelper, () => cancel, automated);
				if (project.Configuration.WaitForDebuggerOrDefault)
				{
					if (!automated)
						consoleHelper.WriteLine("Waiting for debugger to be attached... (press Ctrl+C to abort)");

					while (true)
					{
						if (Debugger.IsAttached)
							break;

						await Task.Delay(10);
					}
				}

				started = true;

				if (project.Configuration.List is not null)
					await ListAssembly(projectAssembly, automated);
				else
					failCount = await projectRunner.Run(projectAssembly, reporterMessageHandler, pipelineStartup);

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
					consoleHelper.WriteLine();
					consoleHelper.Write("Press any key to continue...");
				}

				Console.ReadKey();

				if (!automated)
					consoleHelper.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				consoleHelper.SetForegroundColor(ConsoleColor.Red);

			if (automated)
				consoleHelper.WriteLine(new DiagnosticMessage("error: " + ex.Message).ToJson());
			else
			{
				consoleHelper.WriteLine("error: {0}", ex.Message);

				if (globalInternalDiagnosticMessages)
				{
					if (!noColor)
						consoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

					consoleHelper.WriteLine(ex.StackTrace);
				}
			}

			return ex is ArgumentException ? 3 : 4;
		}
		finally
		{
			if (!noColor)
				consoleHelper.ResetColor();
		}
	}

	async ValueTask ListAssembly(
		XunitProjectAssembly assembly,
		bool automated)
	{
		var (listOption, listFormat) = assembly.Project.Configuration.List!.Value;
		if (automated)
			listFormat = ListFormat.Json;

		var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);
		var projectRunner = new ProjectAssemblyRunner(testAssembly, consoleHelper, () => cancel, automated);
		var testCases = new List<(ITestCase TestCase, bool PassedFilter)>();
		await projectRunner.Discover(assembly, pipelineStartup, testCases: testCases);

		var testCasesDiscovered = testCases.Count;
		var filteredTestCases = testCases.Where(tc => tc.PassedFilter).Select(tc => tc.TestCase).ToList();

		if (listOption != ListOption.Discovery)
		{
			var testCasesByAssembly = new Dictionary<string, List<ITestCase>> { [assemblyFileName] = filteredTestCases };
			ConsoleProjectLister.List(consoleHelper, testCasesByAssembly, listOption, listFormat);
		}
		else
			foreach (var testCase in filteredTestCases)
				consoleHelper.WriteLine(testCase.ToTestCaseDiscovered().ToJson());
	}

	void OnUnhandledException(
		object sender,
		UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			if (automated)
				consoleHelper.WriteLine(ErrorMessage.FromException(ex).ToJson());
			else
				consoleHelper.WriteLine(ex.ToString());
		}
		else
		{
			if (automated)
				consoleHelper.WriteLine(new DiagnosticMessage("Error of unknown type thrown in application domain").ToJson());
			else
				consoleHelper.WriteLine("Error of unknown type thrown in application domain");
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

		consoleHelper.WriteLine(buffer.ToString());
	}

	/// <summary>
	/// Creates a new <see cref="ConsoleRunner"/> instance and runs it via <see cref="EntryPoint"/>.
	/// </summary>
	/// <param name="args">The arguments passed to the application; typically pulled from the Main method.</param>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	// Note: This returns Task instead of ValueTask, because it's called from the injected entry point, and we don't want to
	// assume that the global entry point can use an async Main method (for acceptance testing purposes).
	public static Task<int> Run(string[] args) =>
		new ConsoleRunner(args).EntryPoint();

	/// <summary>
	/// Override this function to change the default output encoding for the system console.
	/// The default is set to <see cref="Encoding.UTF8"/> to support our usage of Unicode
	/// characters in output (for example, the up and down arrows printed for pointers with
	/// mismatched assertion values).
	/// </summary>
	protected virtual void SetOutputEncoding() =>
		Console.OutputEncoding = Encoding.UTF8;
}
