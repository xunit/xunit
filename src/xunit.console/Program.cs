using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        volatile static bool cancel;
        static readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();
        static bool failed;
        static IRunnerLogger logger;
        static IMessageSink reporterMessageHandler;

        [STAThread]
        public static int Main(string[] args)
        {
            try
            {
                var reporters = GetAvailableRunnerReporters();

                if (args.Length == 0 || args[0] == "-?")
                {
                    PrintHeader();
                    PrintUsage(reporters);
                    return 1;
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

                var commandLine = CommandLine.Parse(reporters, args);

                if (commandLine.Debug)
                    Debugger.Launch();

                logger = new ConsoleRunnerLogger(!commandLine.NoColor);
                reporterMessageHandler = commandLine.Reporter.CreateMessageHandler(logger);

                if (!commandLine.NoLogo)
                    PrintHeader();

                var failCount = RunProject(commandLine.Project, commandLine.Serialize, commandLine.ParallelizeAssemblies,
                                           commandLine.ParallelizeTestCollections, commandLine.MaxParallelThreads,
                                           commandLine.DiagnosticMessages, commandLine.NoColor, commandLine.NoAppDomain);

                if (commandLine.Wait)
                {
                    Console.WriteLine();
                    Console.Write("Press any key to continue...");
                    Console.ReadKey();
                    Console.WriteLine();
                }

                return failCount;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"error: {ex.Message}");
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            finally
            {
                Console.ResetColor();
            }
        }

        static List<IRunnerReporter> GetAvailableRunnerReporters()
        {
            var result = new List<IRunnerReporter>();
            var runnerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());

            foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath, f)))
            {
                Type[] types;

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
                    if (type == null || type.IsAbstract || type == typeof(DefaultRunnerReporter) || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
                        continue;
                    var ctor = type.GetConstructor(new Type[0]);
                    if (ctor == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Type {type.FullName} in assembly {dllFile} appears to be a runner reporter, but does not have an empty constructor.");
                        Console.ResetColor();
                        continue;
                    }

                    result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
                }
            }

            return result;
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (ex != null)
                Console.WriteLine(ex.ToString());
            else
                Console.WriteLine("Error of unknown type thrown in application domain");

            Environment.Exit(1);
        }

        static void PrintHeader()
        {
            Console.WriteLine($"xUnit.net Console Runner ({IntPtr.Size * 8}-bit .NET {Environment.Version})");
        }

        static void PrintUsage(IReadOnlyList<IRunnerReporter> reporters)
        {
            var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

            Console.WriteLine("Copyright (C) 2015 Outercurve Foundation.");
            Console.WriteLine();
            Console.WriteLine($"usage: {executableName} <assemblyFile> [configFile] [assemblyFile [configFile]...] [options] [reporter] [resultFormat filename [...]]");
            Console.WriteLine();
            Console.WriteLine("Note: Configuration files must end in .json (for JSON) or .config (for XML)");
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  -nologo                : do not show the copyright message");
            Console.WriteLine("  -nocolor               : do not output results with colors");
            Console.WriteLine("  -noappdomain           : do not use app domains to run test code");
            Console.WriteLine("  -parallel option       : set parallelization based on option");
            Console.WriteLine("                         :   none - turn off all parallelization");
            Console.WriteLine("                         :   collections - only parallelize collections");
            Console.WriteLine("                         :   assemblies - only parallelize assemblies");
            Console.WriteLine("                         :   all - parallelize assemblies & collections");
            Console.WriteLine("  -maxthreads count      : maximum thread count for collection parallelization");
            Console.WriteLine("                         :   default   - run with default (1 thread per CPU thread)");
            Console.WriteLine("                         :   unlimited - run with unbounded thread count");
            Console.WriteLine("                         :   (number)  - limit task thread pool size to 'count'");
            Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
            Console.WriteLine("  -wait                  : wait for input after completion");
            Console.WriteLine("  -diagnostics           : enable diagnostics messages for all test assemblies");
            Console.WriteLine("  -debug                 : launch the debugger to debug the tests");
            Console.WriteLine("  -serialize             : serialize all test cases (for diagnostic purposes only)");
            Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an AND operation");
            Console.WriteLine("  -method \"name\"         : run a given test method (should be fully specified;");
            Console.WriteLine("                         : i.e., 'MyNamespace.MyClass.MyTestMethod')");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -class \"name\"          : run all methods in a given test class (should be fully");
            Console.WriteLine("                         : specified; i.e., 'MyNamespace.MyClass')");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine();

            var switchableReporters = reporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).ToList();
            if (switchableReporters.Count > 0)
            {
                Console.WriteLine("Reporters: (optional, choose only one)");

                foreach (var reporter in switchableReporters.OrderBy(r => r.RunnerSwitch))
                    Console.WriteLine($"  -{reporter.RunnerSwitch.ToLowerInvariant().PadRight(21)} : {reporter.Description}");

                Console.WriteLine();
            }

            Console.WriteLine("Result formats: (optional, choose one or more)");
            TransformFactory.AvailableTransforms.ForEach(
                transform => Console.WriteLine($"  -{$"{transform.CommandLine} <filename>".PadRight(21).Substring(0, 21)} : {transform.Description}")
            );
        }

        static int RunProject(XunitProject project,
                              bool serialize,
                              bool? parallelizeAssemblies,
                              bool? parallelizeTestCollections,
                              int? maxThreadCount,
                              bool diagnosticMessages,
                              bool noColor,
                              bool? noAppDomain)
        {
            XElement assembliesElement = null;
            var clockTime = Stopwatch.StartNew();
            var xmlTransformers = TransformFactory.GetXmlTransformers(project);
            var needsXml = xmlTransformers.Count > 0;
            var consoleLock = new object();

            if (!parallelizeAssemblies.HasValue)
                parallelizeAssemblies = project.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);

            if (needsXml)
                assembliesElement = new XElement("assemblies");

            var originalWorkingFolder = Directory.GetCurrentDirectory();

            using (AssemblyHelper.SubscribeResolve())
            {
                if (parallelizeAssemblies.GetValueOrDefault())
                {
                    var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(consoleLock, assembly, serialize, needsXml, parallelizeTestCollections, maxThreadCount, diagnosticMessages, noColor, noAppDomain, project.Filters)));
                    var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                    foreach (var assemblyElement in results.Where(result => result != null))
                        assembliesElement.Add(assemblyElement);
                }
                else
                {
                    foreach (var assembly in project.Assemblies)
                    {
                        var assemblyElement = ExecuteAssembly(consoleLock, assembly, serialize, needsXml, parallelizeTestCollections, maxThreadCount, diagnosticMessages, noColor, noAppDomain, project.Filters);
                        if (assemblyElement != null)
                            assembliesElement.Add(assemblyElement);
                    }
                }

                clockTime.Stop();

                if (completionMessages.Count > 0)
                    reporterMessageHandler.OnMessage(new TestExecutionSummary(clockTime.Elapsed, completionMessages.OrderBy(kvp => kvp.Key).ToList()));
            }

            Directory.SetCurrentDirectory(originalWorkingFolder);

            xmlTransformers.ForEach(transformer => transformer(assembliesElement));

            return failed ? 1 : completionMessages.Values.Sum(summary => summary.Failed);
        }

        static XElement ExecuteAssembly(object consoleLock,
                                        XunitProjectAssembly assembly,
                                        bool serialize,
                                        bool needsXml,
                                        bool? parallelizeTestCollections,
                                        int? maxThreadCount,
                                        bool diagnosticMessages,
                                        bool noColor,
                                        bool? noAppDomain,
                                        XunitFilters filters)
        {
            if (cancel)
                return null;

            var assemblyElement = needsXml ? new XElement("assembly") : null;

            try
            {
                if (!ValidateFileExists(consoleLock, assembly.AssemblyFilename) || !ValidateFileExists(consoleLock, assembly.ConfigFilename))
                    return null;

                // Turn off pre-enumeration of theories, since there is no theory selection UI in this runner
                assembly.Configuration.PreEnumerateTheories = false;
                assembly.Configuration.DiagnosticMessages |= diagnosticMessages;

                if (noAppDomain.HasValue)
                    assembly.Configuration.UseAppDomain = !noAppDomain.GetValueOrDefault();

                // Setup discovery and execution options with command-line overrides
                var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);
                var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);
                if (maxThreadCount.HasValue)
                    executionOptions.SetMaxParallelThreads(maxThreadCount);
                if (parallelizeTestCollections.HasValue)
                    executionOptions.SetDisableParallelization(!parallelizeTestCollections.GetValueOrDefault());

                var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);
                var diagnosticMessageVisitor = new DiagnosticMessageVisitor(consoleLock, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault, noColor);
                var useAppDomain = assembly.Configuration.UseAppDomainOrDefault;

                using (var controller = new XunitFrontController(useAppDomain, assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy, diagnosticMessageSink: diagnosticMessageVisitor))
                using (var discoveryVisitor = new TestDiscoveryVisitor())
                {
                    // Discover & filter the tests
                    reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryStarting(assembly, useAppDomain, discoveryOptions));

                    controller.Find(false, discoveryVisitor, discoveryOptions);
                    discoveryVisitor.Finished.WaitOne();

                    var testCasesDiscovered = discoveryVisitor.TestCases.Count;
                    var filteredTestCases = discoveryVisitor.TestCases.Where(filters.Filter).ToList();
                    var testCasesToRun = filteredTestCases.Count;

                    reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryFinished(assembly, discoveryOptions, testCasesDiscovered, testCasesToRun));

                    // Run the filtered tests
                    if (testCasesToRun == 0)
                        completionMessages.TryAdd(Path.GetFileName(assembly.AssemblyFilename), new ExecutionSummary());
                    else
                    {
                        if (serialize)
                            filteredTestCases = filteredTestCases.Select(controller.Serialize).Select(controller.Deserialize).ToList();

                        reporterMessageHandler.OnMessage(new TestAssemblyExecutionStarting(assembly, executionOptions));

                        var resultsVisitor = new XmlAggregateVisitor(reporterMessageHandler, completionMessages, assemblyElement, () => cancel);
                        controller.RunTests(filteredTestCases, resultsVisitor, executionOptions);
                        resultsVisitor.Finished.WaitOne();

                        reporterMessageHandler.OnMessage(new TestAssemblyExecutionFinished(assembly, executionOptions, resultsVisitor.ExecutionSummary));
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
                    e = e.InnerException;
                }
            }

            return assemblyElement;
        }

        static bool ValidateFileExists(object consoleLock, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName))
                return true;

            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File not found: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return false;
        }
    }
}
