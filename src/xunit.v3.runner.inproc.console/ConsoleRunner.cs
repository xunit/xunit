using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
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
		IReadOnlyList<IRunnerReporter>? runnerReporters;
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
			this.runnerReporters = runnerReporters.CastOrToReadOnlyList();

			commandLine = CommandLine.Parse(this.testAssembly, this.testAssembly.Location, args);
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

			var globalInternalDiagnosticMessages = false;
			var noColor = false;

			try
			{
				if (commandLine.ParseFault != null)
					ExceptionDispatchInfo.Capture(commandLine.ParseFault).Throw();

				if (runnerReporters == null)
					runnerReporters = GetAvailableRunnerReporters(testAssembly.Location, commandLine.Project.Configuration.NoColorOrDefault);

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

				await using var reporter = commandLine.ChooseReporter(runnerReporters);

				if (commandLine.Project.Configuration.PauseOrDefault)
				{
					Console.Write("Press any key to start execution...");
					Console.ReadKey(true);
					Console.WriteLine();
				}

				if (commandLine.Project.Configuration.DebugOrDefault)
					Debugger.Launch();

				// We will enable "global" internal diagnostic messages if any test assembly wanted them
				globalInternalDiagnosticMessages = commandLine.Project.Assemblies.Any(a => a.Configuration.InternalDiagnosticMessagesOrDefault);
				noColor = commandLine.Project.Configuration.NoColorOrDefault;
				logger = new ConsoleRunnerLogger(!noColor, consoleLock);
				var diagnosticMessageSink = ConsoleDiagnosticMessageSink.ForInternalDiagnostics(consoleLock, globalInternalDiagnosticMessages, noColor);
				var reporterMessageHandler = await reporter.CreateMessageHandler(logger, diagnosticMessageSink);

				if (!reporter.ForceNoLogo && !commandLine.Project.Configuration.NoLogoOrDefault)
					PrintHeader();

				var failCount = 0;

				if (commandLine.Project.Configuration.List != null)
					await ListProject(commandLine.Project);
				else
					failCount = await RunProject(commandLine.Project, reporterMessageHandler);

				if (cancel)
					return -1073741510;    // 0xC000013A: The application terminated as a result of a CTRL+C

				if (commandLine.Project.Configuration.WaitOrDefault)
				{
					Console.WriteLine();
					Console.Write("Press any key to continue...");
					Console.ReadKey();
					Console.WriteLine();
				}

				return commandLine.Project.Configuration.IgnoreFailures == true || failCount == 0 ? 0 : 1;
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

		List<IRunnerReporter> GetAvailableRunnerReporters(
			string? testAssemblyFileName,
			bool noColor)
		{
			var result = new List<IRunnerReporter>();

			var runnerPath = string.IsNullOrWhiteSpace(testAssemblyFileName) ? null : Path.GetDirectoryName(testAssemblyFileName);
			if (!string.IsNullOrWhiteSpace(runnerPath))
			{
				foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath, f)))
				{
					Type?[] types;

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

					foreach (var type in types)
					{
						if (type == null || type.IsAbstract || type.GetCustomAttribute<HiddenRunnerReporterAttribute>() != null || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
							continue;
						var ctor = type.GetConstructor(new Type[0]);
						if (ctor == null)
						{
							if (!noColor)
								ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);
							Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");
							if (!noColor)
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
			Console.WriteLine("General options");
			Console.WriteLine();
			Console.WriteLine("  -culture <option>     : run tests under the given culture");
			Console.WriteLine("                        :   default   - run with default operating system culture");
			Console.WriteLine("                        :   invariant - run with the invariant culture");
			Console.WriteLine("                        :   (string)  - run with the given culture (f.e., 'en-US')");
			Console.WriteLine("  -debug                : launch the debugger to debug the tests");
			Console.WriteLine("  -diagnostics          : enable diagnostics messages for all test assemblies");
			Console.WriteLine("  -failskips            : convert skipped tests into failures");
			Console.WriteLine("  -ignorefailures       : if tests fail, do not return a failure exit code");
			Console.WriteLine("  -internaldiagnostics  : enable internal diagnostics messages for all test assemblies");
			Console.WriteLine("  -list <option>        : list information about the test assemblies rather than running tests (implies -nologo)");
			Console.WriteLine("                        : note: you can add '/json' to the end of any option to get the listing in JSON format");
			Console.WriteLine("                        :   classes - list class names of every class which contains tests");
			Console.WriteLine("                        :   full    - list complete discovery data");
			Console.WriteLine("                        :   methods - list class+method names of every method which is a test");
			Console.WriteLine("                        :   tests   - list just the display name of all tests");
			Console.WriteLine("                        :   traits  - list the set of trait name/value pairs used in the test assemblies");
			Console.WriteLine("  -maxthreads <option>  : maximum thread count for collection parallelization");
			Console.WriteLine("                        :   default   - run with default (1 thread per CPU thread)");
			Console.WriteLine("                        :   unlimited - run with unbounded thread count");
			Console.WriteLine("                        :   (integer) - use exactly this many threads (f.e., '2' = 2 threads)");
			Console.WriteLine("                        :   (float)x  - use a multiple of CPU threads (f.e., '2.0x' = 2.0 * the number of CPU threads)");
			Console.WriteLine("  -noautoreporters      : do not allow reporters to be auto-enabled by environment");
			Console.WriteLine("                        : (for example, auto-detecting TeamCity or AppVeyor)");
			Console.WriteLine("  -nocolor              : do not output results with colors");
			Console.WriteLine("  -nologo               : do not show the copyright message");
			Console.WriteLine("  -pause                : wait for input before running tests");
			Console.WriteLine("  -parallel <option>    : set parallelization based on option");
			Console.WriteLine("                        :   none        - turn off all parallelization");
			Console.WriteLine("                        :   collections - only parallelize collections");
			Console.WriteLine("  -preenumeratetheories : enable theory pre-enumeration (disabled by default)");
			Console.WriteLine("  -stoponfail           : stop on first test failure");
			Console.WriteLine("  -wait                 : wait for input after completion");
			Console.WriteLine();
			// TODO: Should we offer a more flexible (but harder to use?) generalized filtering system?
			Console.WriteLine("Filtering (optional, choose one or more)");
			Console.WriteLine("If more than one filter type is specified, cross-filter type filters act as an AND operation");
			Console.WriteLine();
			Console.WriteLine("  -class \"name\"         : run all methods in a given test class (should be fully");
			Console.WriteLine("                        : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -noclass \"name\"       : do not run any methods in a given test class (should be fully");
			Console.WriteLine("                        : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -method \"name\"        : run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                        : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nomethod \"name\"      : do not run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                        : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -namespace \"name\"     : run all methods in a given namespace (i.e.,");
			Console.WriteLine("                        : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nonamespace \"name\"   : do not run any methods in a given namespace (i.e.,");
			Console.WriteLine("                        : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -trait \"name=value\"   : only run tests with matching name/value traits");
			Console.WriteLine("                        : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -notrait \"name=value\" : do not run tests with matching name/value traits");
			Console.WriteLine("                        : if specified more than once, acts as an AND operation");
			Console.WriteLine();

			if (runnerReporters?.Count > 0)
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

			if (TransformFactory.AvailableTransforms.Count != 0)
			{
				Console.WriteLine("Result formats (optional, choose one or more)");
				Console.WriteLine();

				var longestTransform = TransformFactory.AvailableTransforms.Max(t => t.ID.Length);
				foreach (var transform in TransformFactory.AvailableTransforms.OrderBy(t => t.ID))
					Console.WriteLine($"  -{$"{transform.ID} <filename>".PadRight(longestTransform + 11)} : {transform.Description}");
			}
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

		async ValueTask ListProject(XunitProject project)
		{
			var (listOption, listFormat) = project.Configuration.List!.Value;
			var nullMessageSink = new _NullMessageSink();
			var testCasesByAssembly = new Dictionary<string, List<_TestCaseDiscovered>>();

			foreach (var assembly in project.Assemblies)
			{
				var assemblyFileName = Guard.ArgumentNotNull("assembly.AssemblyFilename", assembly.AssemblyFilename);

				// Default to false for console runners
				assembly.Configuration.PreEnumerateTheories ??= false;

				// Setup discovery options with command line overrides
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);

				var assemblyDisplayName = assembly.AssemblyDisplayName;

				var assemblyInfo = new ReflectionAssemblyInfo(testAssembly);

				await using var disposalTracker = new DisposalTracker();
				var testFramework = ExtensibilityPointFactory.GetTestFramework(nullMessageSink, assemblyInfo);
				disposalTracker.Add(testFramework);

				var discoverySink = new TestDiscoverySink(() => cancel);

				// Discover & filter the tests
				var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
				testDiscoverer.Find(discoverySink, discoveryOptions);
				discoverySink.Finished.WaitOne();

				var testCasesDiscovered = discoverySink.TestCases.Count;
				var filteredTestCases = discoverySink.TestCases.Where(assembly.Configuration.Filters.Filter).ToList();

				testCasesByAssembly.Add(assemblyFileName, filteredTestCases);
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

			if (needsXml)
				assembliesElement = new XElement("assemblies");

			var originalWorkingFolder = Directory.GetCurrentDirectory();

			var assembly = project.Assemblies.Single();
			var assemblyElement = await ExecuteAssembly(
				assembly,
				needsXml,
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

				var assemblyDisplayName = assembly.AssemblyDisplayName;
				var noColor = assembly.Project.Configuration.NoColorOrDefault;
				var diagnosticMessageSink = ConsoleDiagnosticMessageSink.ForDiagnostics(consoleLock, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault, noColor);
				var internalDiagnosticsMessageSink = ConsoleDiagnosticMessageSink.ForInternalDiagnostics(consoleLock, assemblyDisplayName, assembly.Configuration.InternalDiagnosticMessagesOrDefault, noColor);
				var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

				var assemblyInfo = new ReflectionAssemblyInfo(testAssembly);

				await using var disposalTracker = new DisposalTracker();
				var testFramework = ExtensibilityPointFactory.GetTestFramework(diagnosticMessageSink, assemblyInfo);
				disposalTracker.Add(testFramework);

				var discoverySink = new TestDiscoverySink(() => cancel);

				// Discover & filter the tests
				var testDiscoverer = testFramework.GetDiscoverer(assemblyInfo);
				var discoveryStarting = new TestAssemblyDiscoveryStarting
				{
					AppDomain = AppDomainOption.NotAvailable,
					Assembly = assembly,
					DiscoveryOptions = discoveryOptions,
					ShadowCopy = false
				};
				reporterMessageHandler.OnMessage(discoveryStarting);

				testDiscoverer.Find(discoverySink, discoveryOptions);
				discoverySink.Finished.WaitOne();

				var testCasesDiscovered = discoverySink.TestCases.Count;
				var filteredTestCases = discoverySink.TestCases.Where(assembly.Configuration.Filters.Filter).ToList();
				var testCasesToRun = filteredTestCases.Count;

				var discoveryFinished = new TestAssemblyDiscoveryFinished
				{
					Assembly = assembly,
					DiscoveryOptions = discoveryOptions,
					TestCasesDiscovered = testCasesDiscovered,
					TestCasesToRun = testCasesToRun
				};
				reporterMessageHandler.OnMessage(discoveryFinished);

				// Run the filtered tests
				if (testCasesToRun == 0)
					testExecutionSummaries.Add(testDiscoverer.TestAssemblyUniqueID, new ExecutionSummary());
				else
				{
					var executionStarting = new TestAssemblyExecutionStarting
					{
						Assembly = assembly,
						ExecutionOptions = executionOptions
					};
					reporterMessageHandler.OnMessage(executionStarting);

					IExecutionSink resultsSink = new DelegatingExecutionSummarySink(reporterMessageHandler, () => cancel);
					if (assemblyElement != null)
						resultsSink = new DelegatingXmlCreationSink(resultsSink, assemblyElement);
					if (longRunningSeconds > 0)
						resultsSink = new DelegatingLongRunningTestDetectionSink(resultsSink, TimeSpan.FromSeconds(longRunningSeconds), diagnosticMessageSink);
					if (assembly.Configuration.FailSkipsOrDefault)
						resultsSink = new DelegatingFailSkipSink(resultsSink);

					using (resultsSink)
					{
						var executor = testFramework.GetExecutor(assemblyInfo);

						executor.RunTests(filteredTestCases, resultsSink, executionOptions);
						resultsSink.Finished.WaitOne();

						testExecutionSummaries.Add(testDiscoverer.TestAssemblyUniqueID, resultsSink.ExecutionSummary);

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
