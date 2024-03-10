using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole.TestPlatform;
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
	volatile bool cancel;
	readonly object consoleLock;
	bool executed;
	IRunnerLogger? logger;
	IReadOnlyList<IRunnerReporter>? runnerReporters;
	readonly Assembly testAssembly;

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
				ProjectRunner.PrintHeader();

				Console.WriteLine("Copyright (C) .NET Foundation.");
				Console.WriteLine();
				Console.WriteLine("usage: [:seed] [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");

				commandLine.PrintUsage();
				return 2;
			}

			var project = commandLine.Parse();

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

			var globalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.DiagnosticMessagesOrDefault);
			globalInternalDiagnosticMessages = project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);
			noColor = project.Configuration.NoColorOrDefault;
			logger = new ConsoleRunnerLogger(!noColor, consoleLock);
			var globalDiagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleLock, noColor, globalDiagnosticMessages, globalInternalDiagnosticMessages);
			var reporter = project.RunnerReporter;
			var reporterMessageHandler = await reporter.CreateMessageHandler(logger, globalDiagnosticMessageSink);

			if (project.Configuration.InternalMSBuildNode is not null)
			{
				string[] testApplicationArgs = ["--internal-msbuild-node", project.Configuration.InternalMSBuildNode];
				var builder = await TestApplication.CreateBuilderAsync(testApplicationArgs);
				Microsoft.Testing.Platform.MSBuild.TestingPlatformBuilderHook.AddExtensions(builder, testApplicationArgs);
				TestPlatformTestFramework.Register(reporterMessageHandler, builder, project, commandLine.ParseWarnings, consoleLock);
				using var app = await builder.BuildAsync();
				return await app.RunAsync();
			}

			if (!reporter.ForceNoLogo && !project.Configuration.NoLogoOrDefault)
				ProjectRunner.PrintHeader();

			foreach (string warning in commandLine.ParseWarnings)
				logger.LogWarning(warning);

			var projectRunner = new ProjectRunner(consoleLock, () => cancel);
			var failCount = 0;

			if (project.Configuration.List is not null)
				await ListProject(project);
			else
				failCount = await projectRunner.RunProject(project, reporterMessageHandler);

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

			Console.WriteLine("error: {0}", ex.Message);

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

	async ValueTask ListProject(XunitProject project)
	{
		var (listOption, listFormat) = project.Configuration.List!.Value;
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
			Console.WriteLine(ex.ToString());
		else
			Console.WriteLine("Error of unknown type thrown in application domain");

		Environment.Exit(1);
	}

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

	/// <summary>
	/// Override this function to change the default output encoding for the system console.
	/// The default is set to <see cref="Encoding.UTF8"/> to support our usage of Unicode
	/// characters in output (for example, the up and down arrows printed for pointers with
	/// mismatches assertion values).
	/// </summary>
	protected virtual void SetOutputEncoding() =>
		Console.OutputEncoding = Encoding.UTF8;
}
