using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        volatile static bool cancel;
        static readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();
        static readonly ConsoleLogger consoleLogger = new ConsoleLogger();
        static bool failed;
        static readonly TeamCityDisplayNameFormatter teamCityDisplayNameFormatter = new TeamCityDisplayNameFormatter();

        [STAThread]
        public static int Main(string[] args)
        {
            var originalForegroundColor = Console.ForegroundColor;

            try
            {
                if (args.Length == 0 || args[0] == "-?")
                {
                    PrintUsage();
                    PrintHeader();
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
                if (!defaultDirectory.EndsWith(new String(new[] { Path.DirectorySeparatorChar })))
                    defaultDirectory += Path.DirectorySeparatorChar;

                var commandLine = CommandLine.Parse(args);

                if (commandLine.Debug)
                    Debugger.Launch();

                if (!commandLine.NoLogo)
                    PrintHeader();

                var failCount = RunProject(defaultDirectory, commandLine.Project, commandLine.Quiet, commandLine.TeamCity, commandLine.AppVeyor,
                                           commandLine.ParallelizeAssemblies, commandLine.ParallelizeTestCollections,
                                           commandLine.MaxParallelThreads);

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
                Console.WriteLine("error: {0}", ex.Message);
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine("{0}", ex.Message);
                return 1;
            }
            finally
            {
                Console.ForegroundColor = originalForegroundColor;
            }
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
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("xUnit.net console test runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);
            Console.WriteLine("Copyright (C) 2015 Outercurve Foundation.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void PrintUsage()
        {
            var executableName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().GetLocalCodeBase());

            Console.WriteLine("usage: {0} <assemblyFile> [configFile] [assemblyFile [configFile]...] [options]", executableName);
            Console.WriteLine();
            Console.WriteLine("Note: Configuration files must end in .config");
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  -parallel option       : set parallelization based on option");
            Console.WriteLine("                         :   none - turn off all parallelization");
            Console.WriteLine("                         :   collections - only parallelize collections");
            Console.WriteLine("                         :   assemblies - only parallelize assemblies");
            Console.WriteLine("                         :   all - parallelize assemblies & collections");
            Console.WriteLine("  -maxthreads count      : maximum thread count for collection parallelization");
            Console.WriteLine("                         :   0 - run with unbounded thread count");
            Console.WriteLine("                         :   >0 - limit task thread pool size to 'count'");
            Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
            Console.WriteLine("  -teamcity              : forces TeamCity mode (normally auto-detected)");
            Console.WriteLine("  -appveyor              : forces AppVeyor CI mode (normally auto-detected)");
            Console.WriteLine("  -nologo                : do not show the copyright message");
            Console.WriteLine("  -quiet                 : do not show progress messages");
            Console.WriteLine("  -wait                  : wait for input after completion");
            Console.WriteLine("  -debug                 : launch the debugger to debug the tests");
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

            TransformFactory.AvailableTransforms.ForEach(
                transform => Console.WriteLine("  {0} : {1}",
                                               String.Format("-{0} <filename>", transform.CommandLine).PadRight(22).Substring(0, 22),
                                               transform.Description)
            );
        }

        static int RunProject(string defaultDirectory, XunitProject project, bool quiet, bool teamcity, bool appVeyor, bool? parallelizeAssemblies, bool? parallelizeTestCollections, int? maxThreadCount)
        {
            XElement assembliesElement = null;
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
                var clockTime = Stopwatch.StartNew();

                if (parallelizeAssemblies.GetValueOrDefault())
                {
                    var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(consoleLock, defaultDirectory, assembly, quiet, needsXml, teamcity, appVeyor, parallelizeTestCollections, maxThreadCount, project.Filters)));
                    var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                    foreach (var assemblyElement in results.Where(result => result != null))
                        assembliesElement.Add(assemblyElement);
                }
                else
                {
                    foreach (var assembly in project.Assemblies)
                    {
                        var assemblyElement = ExecuteAssembly(consoleLock, defaultDirectory, assembly, quiet, needsXml, teamcity, appVeyor, parallelizeTestCollections, maxThreadCount, project.Filters);
                        if (assemblyElement != null)
                            assembliesElement.Add(assemblyElement);
                    }
                }

                clockTime.Stop();

                if (completionMessages.Count > 0)
                {
                    if (!quiet)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.WriteLine("=== TEST EXECUTION SUMMARY ===");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    var totalTestsRun = completionMessages.Values.Sum(summary => summary.Total);
                    var totalTestsFailed = completionMessages.Values.Sum(summary => summary.Failed);
                    var totalTestsSkipped = completionMessages.Values.Sum(summary => summary.Skipped);
                    var totalTime = completionMessages.Values.Sum(summary => summary.Time).ToString("0.000s");
                    var totalErrors = completionMessages.Values.Sum(summary => summary.Errors);
                    var longestAssemblyName = completionMessages.Keys.Max(key => key.Length);
                    var longestTotal = totalTestsRun.ToString().Length;
                    var longestFailed = totalTestsFailed.ToString().Length;
                    var longestSkipped = totalTestsSkipped.ToString().Length;
                    var longestTime = totalTime.Length;
                    var longestErrors = totalErrors.ToString().Length;

                    foreach (var message in completionMessages.OrderBy(m => m.Key))
                        Console.WriteLine("   {0}  Total: {1}, Errors: {2}, Failed: {3}, Skipped: {4}, Time: {5}",
                                          message.Key.PadRight(longestAssemblyName),
                                          message.Value.Total.ToString().PadLeft(longestTotal),
                                          message.Value.Errors.ToString().PadLeft(longestErrors),
                                          message.Value.Failed.ToString().PadLeft(longestFailed),
                                          message.Value.Skipped.ToString().PadLeft(longestSkipped),
                                          message.Value.Time.ToString("0.000s").PadLeft(longestTime));

                    if (completionMessages.Count > 1)
                        Console.WriteLine("   {0}         {1}          {2}          {3}           {4}        {5}" + Environment.NewLine +
                                          "           {6} {7}          {8}          {9}           {10}        {11} ({12})",
                                          " ".PadRight(longestAssemblyName),
                                          "-".PadRight(longestTotal, '-'),
                                          "-".PadRight(longestErrors, '-'),
                                          "-".PadRight(longestFailed, '-'),
                                          "-".PadRight(longestSkipped, '-'),
                                          "-".PadRight(longestTime, '-'),
                                          "GRAND TOTAL:".PadLeft(longestAssemblyName),
                                          totalTestsRun,
                                          totalErrors,
                                          totalTestsFailed,
                                          totalTestsSkipped,
                                          totalTime,
                                          clockTime.Elapsed.TotalSeconds.ToString("0.000s"));

                }
            }

            Directory.SetCurrentDirectory(originalWorkingFolder);

            xmlTransformers.ForEach(transformer => transformer(assembliesElement));

            return failed ? 1 : completionMessages.Values.Sum(summary => summary.Failed);
        }

        static XmlTestExecutionVisitor CreateVisitor(object consoleLock, bool quiet, string defaultDirectory, XElement assemblyElement, bool teamCity, bool appVeyor)
        {
            if (teamCity)
                return new TeamCityVisitor(consoleLogger, assemblyElement, () => cancel, displayNameFormatter: teamCityDisplayNameFormatter);
            else if (appVeyor)
                return new AppVeyorVisitor(consoleLock, defaultDirectory, assemblyElement, () => cancel, completionMessages);

            return new StandardOutputVisitor(consoleLock, quiet, defaultDirectory, assemblyElement, () => cancel, completionMessages);
        }

        static XElement ExecuteAssembly(object consoleLock, string defaultDirectory, XunitProjectAssembly assembly, bool quiet, bool needsXml, bool teamCity, bool appVeyor, bool? parallelizeTestCollections, int? maxThreadCount, XunitFilters filters)
        {
            if (cancel)
                return null;

            var assemblyElement = needsXml ? new XElement("assembly") : null;

            try
            {
                if (!ValidateFileExists(consoleLock, assembly.AssemblyFilename) || !ValidateFileExists(consoleLock, assembly.ConfigFilename))
                    return null;

                var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);
                var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);
                if (maxThreadCount.HasValue)
                    executionOptions.SetMaxParallelThreads(maxThreadCount.GetValueOrDefault());
                if (parallelizeTestCollections.HasValue)
                    executionOptions.SetDisableParallelization(!parallelizeTestCollections.GetValueOrDefault());

                var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);

                lock (consoleLock)
                {
                    if (assembly.Configuration.DiagnosticMessagesOrDefault)
                        Console.WriteLine("Discovering: {0} (method display = {1}, parallel test collections = {2}, max threads = {3})",
                                          assemblyDisplayName,
                                          discoveryOptions.GetMethodDisplayOrDefault(),
                                          !executionOptions.GetDisableParallelizationOrDefault(),
                                          executionOptions.GetMaxParallelThreadsOrDefault());
                    else if (!quiet)
                        Console.WriteLine("Discovering: {0}", assemblyDisplayName);
                }

                var diagnosticMessageVisitor = new DiagnosticMessageVisitor(consoleLock, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault);

                using (var controller = new XunitFrontController(assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy, diagnosticMessageSink: diagnosticMessageVisitor))
                using (var discoveryVisitor = new TestDiscoveryVisitor())
                {
                    controller.Find(includeSourceInformation: false, messageSink: discoveryVisitor, discoveryOptions: discoveryOptions);
                    discoveryVisitor.Finished.WaitOne();

                    if (!quiet)
                        lock (consoleLock)
                            Console.WriteLine("Discovered:  {0}", Path.GetFileNameWithoutExtension(assembly.AssemblyFilename));

                    var resultsVisitor = CreateVisitor(consoleLock, quiet, defaultDirectory, assemblyElement, teamCity, appVeyor);
                    var filteredTestCases = discoveryVisitor.TestCases.Where(filters.Filter).ToList();
                    if (filteredTestCases.Count == 0)
                    {
                        lock (consoleLock)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("ERROR:       {0} has no tests to run", Path.GetFileNameWithoutExtension(assembly.AssemblyFilename));
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    else
                    {
                        controller.RunTests(filteredTestCases, resultsVisitor, executionOptions);
                        resultsVisitor.Finished.WaitOne();
                    }
                }
            }
            catch (Exception ex)
            {
                failed = true;

                var e = ex;
                while (e != null)
                {
                    Console.WriteLine("{0}: {1}", e.GetType().FullName, e.Message);
                    e = e.InnerException;
                }
            }

            return assemblyElement;
        }

        static bool ValidateFileExists(object consoleLock, string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName) || File.Exists(fileName))
                return true;

            lock (consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File not found: {0}", fileName);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            return false;
        }
    }
}
