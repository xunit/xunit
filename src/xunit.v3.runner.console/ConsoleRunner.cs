using System;
using System.Collections.Concurrent;
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
using Xunit.v3;

namespace Xunit.Runner.SystemConsole
{
	class ConsoleRunner
	{
		string[] args;
		volatile bool cancel;
		CommandLine commandLine;
		readonly object consoleLock = new object();
		readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();
		bool failed;
		IRunnerLogger? logger;

		public ConsoleRunner(
			string[] args,
			object? consoleLock = null)
		{
			this.args = Guard.ArgumentNotNull(nameof(args), args);
			this.consoleLock = consoleLock ?? new object();

			commandLine = CommandLine.Parse(args);
		}

		public async ValueTask<int> EntryPoint()
		{
			var globalInternalDiagnosticMessages = false;
			var noColor = false;

			try
			{
				if (commandLine.ParseFault != null)
					ExceptionDispatchInfo.Capture(commandLine.ParseFault).Throw();

				var reporters = GetAvailableRunnerReporters();

				if (args.Length == 0 || args[0] == "-?" || args[0] == "/?" || args[0] == "-h" || args[0] == "--help")
				{
					PrintHeader();
					PrintUsage(reporters);
					return 2;
				}

				if (commandLine.Project.Assemblies.Count == 0)
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

				await using var reporter = commandLine.ChooseReporter(reporters);

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

				var failCount = await RunProject(
					commandLine.Project,
					reporterMessageHandler
				);

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

		List<IRunnerReporter> GetAvailableRunnerReporters()
		{
			var result = new List<IRunnerReporter>();

			var runnerPath = Path.GetDirectoryName(typeof(Program).Assembly.Location);

			if (runnerPath != null)
				foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath!, f)))
				{
					Type?[] types;

					try
					{
						var assembly = Assembly.LoadFile(dllFile);
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
							ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);
							Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");
							ConsoleHelper.ResetColor();
							continue;
						}

						result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
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

		void PrintHeader()
		{
#if NET472
			var platformSuffix = $"net472";
#elif NET48
			var platformSuffix = $"net48";
#else
#error Unknown target framework
#endif

			Console.WriteLine($"xUnit.net v3 Console Runner v{ThisAssembly.AssemblyInformationalVersion} ({IntPtr.Size * 8}-bit {RuntimeInformation.FrameworkDescription} [{platformSuffix}])");
		}

		void PrintUsage(IReadOnlyList<IRunnerReporter> reporters)
		{
			var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

			Console.WriteLine("Copyright (C) .NET Foundation.");
			Console.WriteLine();
			Console.WriteLine($"usage: {executableName} <assemblyFile> [configFile] [assemblyFile [configFile]...] [options] [reporter] [resultFormat filename [...]]");
			Console.WriteLine();
			Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
			Console.WriteLine("      XML is supported for v1 and v2 only, on .NET Framework only");
			Console.WriteLine("      JSON is supported for v2 and later, on all supported plaforms");
			Console.WriteLine();
			Console.WriteLine("General options");
			Console.WriteLine();
			Console.WriteLine("  -debug                 : launch the debugger to debug the tests");
			Console.WriteLine("  -diagnostics           : enable diagnostics messages for all test assemblies");
			Console.WriteLine("  -failskips             : convert skipped tests into failures");
			Console.WriteLine("  -ignorefailures        : if tests fail, do not return a failure exit code");
			Console.WriteLine("  -internaldiagnostics   : enable internal diagnostics messages for all test assemblies");
			Console.WriteLine("  -maxthreads <option>   : maximum thread count for collection parallelization");
			Console.WriteLine("                         :   default   - run with default (1 thread per CPU thread)");
			Console.WriteLine("                         :   unlimited - run with unbounded thread count");
			Console.WriteLine("                         :   (integer) - use exactly this many threads (f.e., '2' = 2 threads)");
			Console.WriteLine("                         :   (float)x  - use a multiple of CPU threads (f.e., '2.0x' = 2.0 * the number of CPU threads)");
			Console.WriteLine("  -noautoreporters       : do not allow reporters to be auto-enabled by environment");
			Console.WriteLine("                         : (for example, auto-detecting TeamCity or AppVeyor)");
			Console.WriteLine("  -nocolor               : do not output results with colors");
			Console.WriteLine("  -nologo                : do not show the copyright message");
			Console.WriteLine("  -parallel <option>     : set parallelization based on option");
			Console.WriteLine("                         :   none        - turn off all parallelization");
			Console.WriteLine("                         :   collections - only parallelize collections");
			Console.WriteLine("                         :   assemblies  - only parallelize assemblies");
			Console.WriteLine("                         :   all         - parallelize assemblies & collections");
			Console.WriteLine("  -pause                 : wait for input before running tests");
			Console.WriteLine("  -preenumeratetheories  : enable theory pre-enumeration (disabled by default)");
			Console.WriteLine("  -stoponfail            : stop on first test failure");
			Console.WriteLine("  -wait                  : wait for input after completion");
			Console.WriteLine();
			Console.WriteLine("Options for .NET Framework projects");
			Console.WriteLine();
			Console.WriteLine("  -appdomains <option> : choose an app domain mode");
			Console.WriteLine("                       :   required    - force app domains on");
			Console.WriteLine("                       :   denied      - force app domains off");
			Console.WriteLine("                       :   ifavailable - use app domains if they're available");
			Console.WriteLine("  -noshadow            : do not shadow copy assemblies");
			Console.WriteLine();
			// TODO: Should we offer a more flexible (but harder to use?) generalized filtering system?
			Console.WriteLine("Filtering (optional, choose one or more)");
			Console.WriteLine("If more than one filter type is specified, cross-filter type filters act as an AND operation");
			Console.WriteLine();
			Console.WriteLine("  -class \"name\"          : run all methods in a given test class (should be fully");
			Console.WriteLine("                         : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                         : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -noclass \"name\"        : do not run any methods in a given test class (should be fully");
			Console.WriteLine("                         : specified; i.e., 'MyNamespace.MyClass')");
			Console.WriteLine("                         : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -method \"name\"         : run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                         : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                         : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nomethod \"name\"       : do not run a given test method (can be fully specified or use a wildcard;");
			Console.WriteLine("                         : i.e., 'MyNamespace.MyClass.MyTestMethod' or '*.MyTestMethod')");
			Console.WriteLine("                         : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -namespace \"name\"      : run all methods in a given namespace (i.e.,");
			Console.WriteLine("                         : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                         : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -nonamespace \"name\"    : do not run any methods in a given namespace (i.e.,");
			Console.WriteLine("                         : 'MyNamespace.MySubNamespace')");
			Console.WriteLine("                         : if specified more than once, acts as an AND operation");
			Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
			Console.WriteLine("                         : if specified more than once, acts as an OR operation");
			Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
			Console.WriteLine("                         : if specified more than once, acts as an AND operation");
			Console.WriteLine();

			if (reporters.Count > 0)
			{
				Console.WriteLine("Reporters (optional, choose only one)");
				Console.WriteLine();

				var longestSwitch = reporters.Max(r => r.RunnerSwitch?.Length ?? 0);

				foreach (var switchableReporter in reporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.RunnerSwitch))
					Console.WriteLine($"  -{switchableReporter.RunnerSwitch!.ToLowerInvariant().PadRight(longestSwitch)} : {switchableReporter.Description}");

				foreach (var environmentalReporter in reporters.Where(r => string.IsNullOrWhiteSpace(r.RunnerSwitch)).OrderBy(r => r.Description))
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

		async ValueTask<int> RunProject(
			XunitProject project,
			_IMessageSink reporterMessageHandler)
		{
			XElement? assembliesElement = null;
			var clockTime = Stopwatch.StartNew();
			var xmlTransformers = TransformFactory.GetXmlTransformers(project);
			var needsXml = xmlTransformers.Count > 0;
			// TODO: Parallelize the ones that will parallelize, and then run the rest sequentially?
			var parallelizeAssemblies = project.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);

			if (needsXml)
				assembliesElement = new XElement("assemblies");

			var originalWorkingFolder = Directory.GetCurrentDirectory();

			if (parallelizeAssemblies)
			{
				var tasks = project.Assemblies.Select(
					assembly => Task.Run(
						() => ExecuteAssembly(
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
					var assemblyElement = await ExecuteAssembly(
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
				var assemblyFileName = Guard.ArgumentNotNull("assembly.AssemblyFilename", assembly.AssemblyFilename);

				// Setup discovery and execution options with command-line overrides
				var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
				var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);

				// The normal default is true here, but we want it to be false for us by default
				if (!assembly.Configuration.PreEnumerateTheories.HasValue)
					discoveryOptions.SetPreEnumerateTheories(false);

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

		bool ValidateFileExists(object consoleLock, string? fileName)
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
