using System;
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
using Xunit.Runner.v2;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole
{
	/// <summary>
	/// This class is the entry point for the in-process console-based runner used for
	/// xUnit.net v3 test projects.
	/// </summary>
	public class ConsoleRunner
	{
		string[] args;
		volatile bool cancel;
		CommandLine commandLine;
		readonly object consoleLock;
		bool executed = false;
		bool failed;
		IRunnerLogger? logger;
		IReadOnlyList<IRunnerReporter> runnerReporters;
		Assembly testAssembly;
		TestExecutionSummaries testExecutionSummaries = new TestExecutionSummaries();

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
			this.args = Guard.ArgumentNotNull(nameof(args), args);
			this.testAssembly = Guard.NotNull("Assembly.GetEntryAssembly() returned null", testAssembly ?? Assembly.GetEntryAssembly());
			this.consoleLock = consoleLock ?? new object();

			commandLine = CommandLine.Parse(this.testAssembly.Location, args);

			var projectAssembly = commandLine.Project.Assemblies.SingleOrDefault();
			Guard.NotNull("commandLine.Project.Assemblies is empty", projectAssembly);

			this.runnerReporters =
				runnerReporters != null
					? runnerReporters.ToList()
					: GetAvailableRunnerReporters(projectAssembly.AssemblyFilename);
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

			try
			{
				if (args.Length > 0 && (args[0] == "-?" || args[0] == "/?" || args[0] == "-h" || args[0] == "--help"))
				{
					PrintHeader();
					PrintUsage();
					return 2;
				}

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

				var defaultDirectory = Directory.GetCurrentDirectory();
				if (!defaultDirectory.EndsWith(new string(new[] { Path.DirectorySeparatorChar }), StringComparison.Ordinal))
					defaultDirectory += Path.DirectorySeparatorChar;

				var reporter = commandLine.ChooseReporter(runnerReporters);

				if (commandLine.Pause)
				{
					Console.Write("Press any key to start execution...");
					Console.ReadKey(true);
					Console.WriteLine();
				}

				if (commandLine.Debug)
					Debugger.Launch();

				logger = new ConsoleRunnerLogger(!commandLine.NoColor, consoleLock);
				var reporterMessageHandler = reporter.CreateMessageHandler(logger);

				if (!commandLine.NoLogo)
					PrintHeader();

				var failCount = await RunProject(
					commandLine.Project,
					commandLine.ParallelizeTestCollections,
					commandLine.MaxParallelThreads,
					commandLine.DiagnosticMessages,
					commandLine.NoColor,
					commandLine.FailSkips,
					commandLine.StopOnFail,
					commandLine.InternalDiagnosticMessages,
					reporterMessageHandler
				);

				if (cancel)
					return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C

				if (commandLine.Wait)
				{
					Console.WriteLine();
					Console.Write("Press any key to continue...");
					Console.ReadKey();
					Console.WriteLine();
				}

				return failCount > 0 ? 1 : 0;
			}
			catch (Exception ex)
			{
				if (!commandLine.NoColor)
					ConsoleHelper.SetForegroundColor(ConsoleColor.Red);

				Console.WriteLine($"error: {ex.Message}");

				if (commandLine.InternalDiagnosticMessages)
				{
					if (!commandLine.NoColor)
						ConsoleHelper.SetForegroundColor(ConsoleColor.DarkGray);

					Console.WriteLine(ex.StackTrace);
				}

				return ex is ArgumentException ? 3 : 4;
			}
			finally
			{
				if (!commandLine.NoColor)
					ConsoleHelper.ResetColor();
			}
		}

		List<IRunnerReporter> GetAvailableRunnerReporters(string? testAssemblyFileName)
		{
			var result = new List<IRunnerReporter>();

			var runnerPath = string.IsNullOrWhiteSpace(testAssemblyFileName) ? null : Path.GetDirectoryName(testAssemblyFileName);
			if (!string.IsNullOrWhiteSpace(runnerPath))
			{
				foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath, f)))
				{
					Type[]? types;

					try
					{
#if NETFRAMEWORK
						var assembly = Assembly.LoadFile(dllFile);
#else
						var assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));
#endif
						types = assembly.GetTypes();
					}
					catch (ReflectionTypeLoadException ex)
					{
						types = ex.Types;
					}
					catch
					{
						continue;
					}

					foreach (var type in types ?? new Type[0])
					{
						if (type == null || type.IsAbstract || type == typeof(DefaultRunnerReporter) || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
							continue;
						var ctor = type.GetConstructor(new Type[0]);
						if (ctor == null)
						{
							if (!commandLine.NoColor)
								ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);
							Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");
							if (!commandLine.NoColor)
								ConsoleHelper.ResetColor();
							continue;
						}

						result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
					}
				}
			}

			return result;
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

		void PrintHeader() =>
			Console.WriteLine($"xUnit.net v3 In-Process Runner v{ThisAssembly.AssemblyInformationalVersion} ({IntPtr.Size * 8}-bit {RuntimeInformation.FrameworkDescription})");

		void PrintUsage()
		{
			Console.WriteLine("Copyright (C) .NET Foundation.");
			Console.WriteLine();
			Console.WriteLine($"usage: [path/to/configFile.json] [options] [filters] [reporter] [resultFormat filename [...]]");
			Console.WriteLine();
			Console.WriteLine("Options");
			Console.WriteLine();
			Console.WriteLine("  -nologo               : do not show the copyright message");
			Console.WriteLine("  -nocolor              : do not output results with colors");
			Console.WriteLine("  -failskips            : convert skipped tests into failures");
			Console.WriteLine("  -stoponfail           : stop on first test failure");
			Console.WriteLine("  -parallel option      : set parallelization based on option");
			Console.WriteLine("                        :   none        - turn off all parallelization");
			Console.WriteLine("                        :   collections - only parallelize collections");
			Console.WriteLine("  -maxthreads count     : maximum thread count for collection parallelization");
			Console.WriteLine("                        :   default   - run with default (1 thread per CPU thread)");
			Console.WriteLine("                        :   unlimited - run with unbounded thread count");
			Console.WriteLine("                        :   (number)  - limit task thread pool size to 'count'");
			Console.WriteLine("  -wait                 : wait for input after completion");
			Console.WriteLine("  -diagnostics          : enable diagnostics messages for all test assemblies");
			Console.WriteLine("  -internaldiagnostics  : enable internal diagnostics messages for all test assemblies");
			Console.WriteLine("  -pause                : pause before doing any work, to help attach a debugger");
			Console.WriteLine("  -debug                : launch the debugger to debug the tests");
			Console.WriteLine("  -noautoreporters      : do not allow reporters to be auto-enabled by environment");
			Console.WriteLine("                        : (for example, auto-detecting TeamCity or AppVeyor)");
			Console.WriteLine("  -preenumeratetheories : enable theory pre-enumeration (disabled by default)");
			Console.WriteLine();
			// TODO: Should we offer a more flexible (but harder to use?) generalized filtering system?
			Console.WriteLine("Filtering (optional, choose one or more)");
			Console.WriteLine("If more than one filter type is specified, cross-filter type filters act as an AND operation");
			Console.WriteLine();
			Console.WriteLine("  -trait \"name=value\"   : only run tests with matching name/value traits");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -notrait \"name=value\" : do not run tests with matching name/value traits");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -method \"name\"        : run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                        : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nomethod \"name\"      : do not run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                        : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -class \"name\"         : run all methods in a given test class (should be fully");
			Console.WriteLine("                        : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -noclass \"name\"       : do not run any methods in a given test class (should be fully");
			Console.WriteLine("                        : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -namespace \"name\"     : run all methods in a given namespace (i.e.,");
			Console.WriteLine("                        : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nonamespace \"name\"   : do not run any methods in a given namespace (i.e.,");
			Console.WriteLine("                        : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine();

			if (runnerReporters.Count > 0)
			{
				Console.WriteLine("Reporters (optional, choose only one)");
				Console.WriteLine();

				var longestSwitch = runnerReporters.Max(r => r.RunnerSwitch?.Length ?? 0);

				foreach (var switchableReporter in runnerReporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.RunnerSwitch))
					Console.WriteLine($"  -{switchableReporter.RunnerSwitch!.ToLowerInvariant().PadRight(longestSwitch)} : {switchableReporter.Description}");

				foreach (var environmentalReporter in runnerReporters.Where(r => string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.Description))
					Console.WriteLine($"   {"".PadRight(longestSwitch)} : {environmentalReporter.Description} [auto-enabled only]");

				Console.WriteLine();
			}

			Console.WriteLine("Result formats (optional, choose one or more)");
			Console.WriteLine();

			var longestTransform = TransformFactory.AvailableTransforms.Max(t => t.ID.Length);
			foreach (var transform in TransformFactory.AvailableTransforms)
				Console.WriteLine($"  -{$"{transform.ID} <filename>".PadRight(longestTransform + 11)} : {transform.Description}");
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

		async ValueTask<int> RunProject(
			XunitProject project,
			bool? parallelizeTestCollections,
			int? maxThreadCount,
			bool diagnosticMessages,
			bool noColor,
			bool failSkips,
			bool stopOnFail,
			bool internalDiagnosticMessages,
			_IMessageSink reporterMessageHandler)
		{
			XElement? assembliesElement = null;
			var clockTime = Stopwatch.StartNew();
			var xmlTransformers = TransformFactory.GetXmlTransformers(project);
			var needsXml = xmlTransformers.Count > 0;

			if (needsXml)
				assembliesElement = new XElement("assemblies");

			var originalWorkingFolder = Directory.GetCurrentDirectory();

			var assembly = project.Assemblies.Single();
			var assemblyElement = await ExecuteAssembly(
				consoleLock,
				assembly,
				needsXml,
				parallelizeTestCollections,
				maxThreadCount,
				diagnosticMessages,
				noColor,
				failSkips,
				stopOnFail,
				project.Filters,
				internalDiagnosticMessages,
				reporterMessageHandler
			);

			if (assemblyElement != null)
				assembliesElement?.Add(assemblyElement);

			clockTime.Stop();

			if (assembliesElement != null)
				assembliesElement.Add(new XAttribute("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)));

			testExecutionSummaries.ElapsedClockTime = clockTime.Elapsed;
			reporterMessageHandler.OnMessage(testExecutionSummaries);

			Directory.SetCurrentDirectory(originalWorkingFolder);

			if (assembliesElement != null)
				xmlTransformers.ForEach(transformer => transformer(assembliesElement));

			return failed ? 1 : testExecutionSummaries.SummariesByAssemblyUniqueID.Sum(s => s.Summary.Failed + s.Summary.Errors);
		}

		async ValueTask<XElement?> ExecuteAssembly(
			object consoleLock,
			XunitProjectAssembly assembly,
			bool needsXml,
			bool? parallelizeTestCollections,
			int? maxThreadCount,
			bool diagnosticMessages,
			bool noColor,
			bool failSkips,
			bool stopOnFail,
			XunitFilters filters,
			bool internalDiagnosticMessages,
			_IMessageSink reporterMessageHandler)
		{
			if (cancel)
				return null;

			var assemblyElement = needsXml ? new XElement("assembly") : null;

			try
			{
				if (!ValidateFileExists(consoleLock, assembly.ConfigFilename))
					return null;

				assembly.Configuration.PreEnumerateTheories = commandLine.PreEnumerateTheories;
				assembly.Configuration.DiagnosticMessages |= diagnosticMessages;
				assembly.Configuration.InternalDiagnosticMessages |= internalDiagnosticMessages;

				// Setup discovery and execution options with command-line overrides
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
				var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);
				executionOptions.SetStopOnTestFail(stopOnFail);
				if (maxThreadCount.HasValue)
					executionOptions.SetMaxParallelThreads(maxThreadCount);
				if (parallelizeTestCollections.HasValue)
					executionOptions.SetDisableParallelization(!parallelizeTestCollections.GetValueOrDefault());

				var assemblyDisplayName = assembly.AssemblyDisplayName;
				var diagnosticMessageSink = ConsoleDiagnosticMessageSink.ForDiagnostics(consoleLock, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault, noColor);
				var internalDiagnosticsMessageSink = ConsoleDiagnosticMessageSink.ForInternalDiagnostics(consoleLock, assemblyDisplayName, internalDiagnosticMessages, noColor);
				var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

				var assemblyInfo = new ReflectionAssemblyInfo(testAssembly);

				await using var disposalTracker = new DisposalTracker();
				var testFramework = ExtensibilityPointFactory.GetTestFramework(diagnosticMessageSink, assemblyInfo);
				disposalTracker.Add(testFramework);

				var discoverySink = new TestDiscoverySink(() => cancel);

				// Discover & filter the tests
				var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
				reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryStarting(assembly, appDomain: AppDomainOption.NotAvailable, shadowCopy: false, discoveryOptions));

				testDiscoverer.Find(includeSourceInformation: false, discoverySink, discoveryOptions);
				discoverySink.Finished.WaitOne();

				var testCasesDiscovered = discoverySink.TestCases.Count;
				var filteredTestCases = discoverySink.TestCases.Where(filters.Filter).ToList();
				var testCasesToRun = filteredTestCases.Count;

				reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryFinished(assembly, discoveryOptions, testCasesDiscovered, testCasesToRun));

				// Run the filtered tests
				if (testCasesToRun != 0)
				{
					reporterMessageHandler.OnMessage(new TestAssemblyExecutionStarting(assembly, executionOptions));

					IExecutionSink resultsSink = new DelegatingExecutionSummarySink(reporterMessageHandler, () => cancel);
					if (assemblyElement != null)
						resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
					if (longRunningSeconds > 0)
						resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink);
					if (failSkips)
						resultsSink = new DelegatingFailSkipSink(resultsSink);

					var executor = testFramework.GetExecutor(assemblyInfo);
					executor.RunTests(filteredTestCases, resultsSink, executionOptions);
					resultsSink.Finished.WaitOne();

					testExecutionSummaries.Add(testDiscoverer.TestAssemblyUniqueID, resultsSink.ExecutionSummary);

					reporterMessageHandler.OnMessage(new TestAssemblyExecutionFinished(assembly, executionOptions, resultsSink.ExecutionSummary));
					if (stopOnFail && resultsSink.ExecutionSummary.Failed != 0)
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

					if (internalDiagnosticMessages)
						Console.WriteLine(e.StackTrace);

					e = e.InnerException;
				}
			}

			return assemblyElement;
		}

		bool ValidateFileExists(
			object consoleLock,
			string? fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName))
				return true;

			lock (consoleLock)
			{
				ConsoleHelper.SetForegroundColor(ConsoleColor.Red);
				Console.WriteLine($"File not found: {fileName}");
				ConsoleHelper.SetForegroundColor(ConsoleColor.Gray);
			}

			return false;
		}
	}
}
