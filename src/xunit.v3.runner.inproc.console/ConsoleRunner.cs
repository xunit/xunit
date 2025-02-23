using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
	Assembly? testAssembly = null) :
		IDisposable
{
	readonly string[] args = Guard.ArgumentNotNull(args);
	AutomatedMode automatedMode;
	readonly CancellationTokenSource cancellationTokenSource = new();
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
		cancellationTokenSource.Cancel();

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		cancellationTokenSource.Dispose();
	}

	/// <summary>
	/// The entry point to begin running tests.
	/// </summary>
	/// <returns>The return value intended to be returned by the Main method.</returns>
	// The signature of this method is known to Xunit3, so do not change it without also changing
	// the code that invokes it dynamically.
	public async Task<int> EntryPoint(
		TextReader? consoleReader = null,
		TextWriter? consoleWriter = null)
	{
		if (executed)
			throw new InvalidOperationException("The EntryPoint method can only be called once.");

		executed = true;

		if (consoleWriter is null)
			SetOutputEncoding();

		consoleHelper = new(consoleReader ?? Console.In, consoleWriter ?? Console.Out);

		var internalDiagnosticMessages = false;
		var noColor = false;

		try
		{
			var commandLine = new CommandLine(consoleHelper, testAssembly, args);
			var warnings = commandLine.ParseWarnings.ToList();

			if (commandLine.HelpRequested)
			{
				consoleHelper.WriteLine(ProjectAssemblyRunner.Banner);
				consoleHelper.WriteLine("Copyright (C) .NET Foundation.");
				consoleHelper.WriteLine();

				if (warnings.Count > 0)
				{
					foreach (var warning in warnings)
						consoleHelper.WriteLine("Warning: {0}", warning);

					consoleHelper.WriteLine();
				}

				consoleHelper.WriteLine("usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");

				commandLine.PrintUsage();
				return 2;
			}

			SerializationHelper.Instance.AddRegisteredSerializers(testAssembly, warnings);

			// We pick up the -automated flag early, because Parse() can throw and we want to use automated output
			// to report any command line parsing problems.
			var automatedRequested = commandLine.AutomatedRequested;
			if (automatedRequested)
			{
				if (consoleReader is null)
					Console.SetIn(TextReader.Null);
				if (consoleWriter is null)
					Console.SetOut(TextWriter.Null);

				noColor = true;
			}

			var projectAssembly = commandLine.Parse();
			automatedMode = (automatedRequested, projectAssembly.Configuration.SynchronousMessageReporting) switch
			{
				(true, true) => AutomatedMode.Sync,
				(true, _) => AutomatedMode.Async,
				_ => AutomatedMode.Off,
			};
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

			if (project.Configuration.PauseOrDefault && automatedMode == AutomatedMode.Off)
			{
				consoleHelper.Write("Press any key to start execution...");

				Console.ReadKey(true);

				consoleHelper.WriteLine();
			}

			if (project.Configuration.DebugOrDefault)
				Debugger.Launch();

			var diagnosticMessages = projectAssembly.Configuration.DiagnosticMessagesOrDefault;
			internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;

			if (automatedMode == AutomatedMode.Off)
				noColor = project.Configuration.NoColorOrDefault;

			logger = new ConsoleRunnerLogger(!noColor, useAnsiColor, consoleHelper, waitForAcknowledgment: automatedMode == AutomatedMode.Sync);

			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				if (e.ExceptionObject is Exception ex)
				{
					if (automatedMode != AutomatedMode.Off)
						logger.WriteMessage(ErrorMessage.FromException(ex));
					else
						consoleHelper.WriteLine(ex.ToString());
				}
				else
				{
					if (automatedMode != AutomatedMode.Off)
						logger.WriteMessage(new DiagnosticMessage("Error of unknown type thrown in application domain"));
					else
						consoleHelper.WriteLine("Error of unknown type thrown in application domain");
				}

				Environment.Exit(1);
			};

			Console.CancelKeyPress += (sender, e) =>
			{
				if (started && !cancellationTokenSource.IsCancellationRequested)
				{
					if (automatedMode != AutomatedMode.Off)
						logger.WriteMessage(new DiagnosticMessage("Cancellation request received"));
					else
						consoleHelper.WriteLine("Cancelling... (Press Ctrl+C again to terminate)");

					e.Cancel = true;
					cancellationTokenSource.Cancel();
				}
			};

			IMessageSink? diagnosticMessageSink =
				automatedMode != AutomatedMode.Off
					? new AutomatedDiagnosticMessageSink(logger)
					: ConsoleDiagnosticMessageSink.TryCreate(consoleHelper, noColor, diagnosticMessages, internalDiagnosticMessages, assemblyDisplayName: Path.GetFileNameWithoutExtension(testAssembly.Location));

			pipelineStartup = await ProjectAssemblyRunner.InvokePipelineStartup(testAssembly, diagnosticMessageSink);

			var failCount = 0;

			try
			{
				var reporter = automatedMode != AutomatedMode.Off ? new JsonReporter() : project.RunnerReporter;
				var reporterMessageHandler = await reporter.CreateMessageHandler(logger, diagnosticMessageSink);

				if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
					consoleHelper.WriteLine(ProjectAssemblyRunner.Banner);

				foreach (var warning in warnings)
					if (automatedMode != AutomatedMode.Off)
						logger.WriteMessage(new DiagnosticMessage("warning: " + warning));
					else
						logger.LogWarning(warning);

				var projectRunner = new ProjectAssemblyRunner(testAssembly, automatedMode, cancellationTokenSource);
				if (project.Configuration.WaitForDebuggerOrDefault)
				{
					if (automatedMode == AutomatedMode.Off)
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
					await ListAssembly(projectAssembly, logger, cancellationTokenSource);
				else
				{
					// Default to false for console runners
					projectAssembly.Configuration.PreEnumerateTheories ??= false;

					failCount = await projectRunner.Run(projectAssembly, reporterMessageHandler, diagnosticMessageSink, logger, pipelineStartup);
				}

				if (cancellationTokenSource.IsCancellationRequested)
					return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C
			}
			finally
			{
				if (pipelineStartup is not null)
					await pipelineStartup.StopAsync();
			}

			if (project.Configuration.WaitOrDefault && automatedMode == AutomatedMode.Off)
			{
				consoleHelper.WriteLine();
				consoleHelper.Write("Press any key to continue...");

				Console.ReadKey();

				consoleHelper.WriteLine();
			}

			return project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
		}
		catch (Exception ex)
		{
			if (!noColor)
				consoleHelper.SetForegroundColor(ConsoleColor.Red);

			if (automatedMode != AutomatedMode.Off)
				logger?.WriteMessage(new DiagnosticMessage("error: " + ex.Message));
			else
			{
				consoleHelper.WriteLine("error: {0}", ex.Message);

				if (internalDiagnosticMessages)
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
		IRunnerLogger logger,
		CancellationTokenSource cancellationTokenSource)
	{
		var (listOption, listFormat) = assembly.Project.Configuration.List!.Value;
		if (automatedMode != AutomatedMode.Off)
			listFormat = ListFormat.Json;

		// Default to false for console runners
		assembly.Configuration.PreEnumerateTheories ??= false;

		var assemblyFileName = Guard.ArgumentNotNull(assembly.AssemblyFileName);
		var projectRunner = new ProjectAssemblyRunner(testAssembly, automatedMode, cancellationTokenSource);
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
				logger.WriteMessage(testCase.ToTestCaseDiscovered());
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
	public static async Task<int> Run(string[] args)
	{
		using var runner = new ConsoleRunner(args);

		return await runner.EntryPoint();
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
