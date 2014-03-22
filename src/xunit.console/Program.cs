using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static bool failed;
        static readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();

        [STAThread]
        public static int Main(string[] args)
        {
            Console.WriteLine("xUnit.net console test runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);
            Console.WriteLine("Copyright (C) 2014 Outercurve Foundation.");
            Console.WriteLine();

            if (args.Length == 0 || args[0] == "-?")
            {
                PrintUsage();
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

            try
            {
                var defaultDirectory = Directory.GetCurrentDirectory();
                if (!defaultDirectory.EndsWith(new String(new[] { Path.DirectorySeparatorChar })))
                    defaultDirectory += Path.DirectorySeparatorChar;

                var commandLine = CommandLine.Parse(args);

                int failCount = RunProject(defaultDirectory, commandLine.Project, commandLine.TeamCity, commandLine.Silent, commandLine.Parallel);

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

        static void PrintUsage()
        {
            string executableName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            Console.WriteLine("usage: {0} <assemblyFile> [configFile] [options]", executableName);
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  -parallel              : run test assemblies in parallel");
            Console.WriteLine("  -silent                : do not output running test count");
            Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
            Console.WriteLine("  -teamcity              : forces TeamCity mode (normally auto-detected)");
            Console.WriteLine("  -wait                  : wait for input after completion");
            Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an AND operation");

            TransformFactory.AvailableTransforms.ForEach(
                transform => Console.WriteLine("  {0} : {1}",
                                               String.Format("-{0} <filename>", transform.CommandLine).PadRight(22).Substring(0, 22),
                                               transform.Description)
            );
        }

        static int RunProject(string defaultDirectory, XunitProject project, bool teamcity, bool silent, bool parallel)
        {
            XElement assembliesElement = null;
            var xmlTransformers = TransformFactory.GetXmlTransformers(project);
            var needsXml = xmlTransformers.Count > 0;
            var consoleLock = new object();

            if (needsXml)
                assembliesElement = new XElement("assemblies");

            string originalWorkingFolder = Directory.GetCurrentDirectory();

            using (AssemblyHelper.SubscribeResolve())
            {
                if (parallel)
                {
                    var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(consoleLock, defaultDirectory, assembly, needsXml, teamcity, silent)));
                    var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                    foreach (var assemblyElement in results.Where(result => result != null))
                        assembliesElement.Add(assemblyElement);
                }
                else
                {
                    foreach (var assembly in project.Assemblies)
                    {
                        var assemblyElement = ExecuteAssembly(consoleLock, defaultDirectory, assembly, needsXml, teamcity, silent);
                        if (assemblyElement != null)
                            assembliesElement.Add(assemblyElement);
                    }
                }

                if (completionMessages.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("=== TEST EXECUTION SUMMARY ===");
                    int longestAssemblyName = completionMessages.Keys.Max(key => key.Length);
                    int longestTotal = completionMessages.Values.Max(summary => summary.Total.ToString().Length);
                    int longestFailed = completionMessages.Values.Max(summary => summary.Failed.ToString().Length);
                    int longestSkipped = completionMessages.Values.Max(summary => summary.Skipped.ToString().Length);
                    int longestTime = completionMessages.Values.Max(summary => summary.Time.ToString("0.000s").Length);

                    foreach (var message in completionMessages.OrderBy(m => m.Key))
                        Console.WriteLine("   {0}  Total: {1}, Failed: {2}, Skipped: {3}, Time: {4}",
                                          message.Key.PadRight(longestAssemblyName),
                                          message.Value.Total.ToString().PadLeft(longestTotal),
                                          message.Value.Failed.ToString().PadLeft(longestFailed),
                                          message.Value.Skipped.ToString().PadLeft(longestSkipped),
                                          message.Value.Time.ToString("0.000s").PadLeft(longestTime));
                }
            }

            Directory.SetCurrentDirectory(originalWorkingFolder);

            xmlTransformers.ForEach(transformer => transformer(assembliesElement));

            return failed ? 1 : completionMessages.Values.Sum(summary => summary.Failed);
        }

        static XmlTestExecutionVisitor CreateVisitor(object consoleLock, string defaultDirectory, XElement assemblyElement, bool teamCity, bool silent)
        {
            if (teamCity)
                return new TeamCityVisitor(assemblyElement, () => cancel);

            return new StandardOutputVisitor(consoleLock, defaultDirectory, assemblyElement, () => cancel, completionMessages);
        }

        static XElement ExecuteAssembly(object consoleLock, string defaultDirectory, XunitProjectAssembly assembly, bool needsXml, bool teamCity, bool silent)
        {
            if (cancel)
                return null;

            var assemblyElement = needsXml ? new XElement("assembly") : null;

            try
            {
                if (!ValidateFileExists(consoleLock, assembly.AssemblyFilename) || !ValidateFileExists(consoleLock, assembly.ConfigFilename))
                    return null;

                using (var controller = new XunitFrontController(assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy))
                using (var discoveryVisitor = new TestDiscoveryVisitor())
                {
                    controller.Find(includeSourceInformation: false, messageSink: discoveryVisitor, options: new TestFrameworkOptions());
                    discoveryVisitor.Finished.WaitOne();

                    var resultsVisitor = CreateVisitor(consoleLock, defaultDirectory, assemblyElement, teamCity, silent);
                    controller.Run(discoveryVisitor.TestCases, resultsVisitor, new TestFrameworkOptions());
                    resultsVisitor.Finished.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}: {1}", ex.GetType().FullName, ex.Message);
                failed = true;
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
