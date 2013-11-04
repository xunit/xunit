using System;
using System.Collections.Concurrent;
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

        [STAThread]
        public static int Main(string[] args)
        {
            Console.WriteLine("xUnit.net console test runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);
            Console.WriteLine("Copyright (C) 2013 Outercurve Foundation.");

            if (args.Length == 0 || args[0] == "-?")
            {
                PrintUsage();
                return 1;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                var commandLine = CommandLine.Parse(args);

                int failCount = RunProject(commandLine.Project, commandLine.TeamCity, commandLine.Silent, commandLine.Parallel);

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
                Console.WriteLine();
                Console.WriteLine("error: {0}", ex.Message);
                return 1;
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine();
                Console.WriteLine("{0}", ex.Message);
                return 1;
            }
        }

        static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
                Console.WriteLine(ex.ToString());
            else
                Console.WriteLine("Error of unknown type thrown in applicaton domain");

            Environment.Exit(1);
        }

        static void PrintUsage()
        {
            string executableName = Path.GetFileNameWithoutExtension(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            Console.WriteLine();
            Console.WriteLine("usage: {0} <xunitProjectFile> [options]", executableName);
            Console.WriteLine("usage: {0} <assemblyFile> [configFile] [options]", executableName);
            Console.WriteLine();
            Console.WriteLine("Valid options:");
            Console.WriteLine("  -parallel              : run test assemblies in parallel");
            Console.WriteLine("  -silent                : do not output running test count");
            Console.WriteLine("  -teamcity              : forces TeamCity mode (normally auto-detected)");
            Console.WriteLine("  -wait                  : wait for input after completion");
            Console.WriteLine("  -trait \"name=value\"    : only run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  -notrait \"name=value\"  : do not run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an AND operation");
            Console.WriteLine();
            Console.WriteLine("Valid options for assemblies only:");
            Console.WriteLine("  -noshadow              : do not shadow copy assemblies");
            Console.WriteLine("  -xml <filename>        : output results to xUnit.net v2 style XML file");

            foreach (TransformConfigurationElement transform in TransformFactory.GetInstalledTransforms())
            {
                string commandLine = "-" + transform.CommandLine + " <filename>";
                commandLine = commandLine.PadRight(22).Substring(0, 22);

                Console.WriteLine("  {0} : {1}", commandLine, transform.Description);
            }
        }

        static int RunProject(XunitProject project, bool teamcity, bool silent, bool parallel)
        {
            XElement assembliesElement = null;
            var needsXml = project.Output.Count > 0;

            if (needsXml)
                assembliesElement = new XElement("assemblies");

            string originalWorkingFolder = Directory.GetCurrentDirectory();

            using (AssemblyHelper.SubscribeResolve())
            {
                if (parallel)
                {
                    var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(assembly, needsXml, teamcity, silent)));
                    var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                    foreach (var assemblyElement in results.Where(result => result != null))
                        assembliesElement.Add(assemblyElement);
                }
                else
                {
                    foreach (var assembly in project.Assemblies)
                    {
                        var assemblyElement = ExecuteAssembly(assembly, needsXml, teamcity, silent);
                        if (assemblyElement != null)
                            assembliesElement.Add(assemblyElement);
                    }
                }

                if (completionMessages.Count > 0)
                {
                    Console.WriteLine("=== TEST EXECUTION SUMMARY ===");
                    int longestAssemblyName = completionMessages.Keys.Max(key => key.Length);
                    int longestTotal = completionMessages.Values.Max(summary => summary.Total.ToString().Length);
                    int longestFailed = completionMessages.Values.Max(summary => summary.Failed.ToString().Length);
                    int longestSkipped = completionMessages.Values.Max(summary => summary.Skipped.ToString().Length);

                    foreach (var message in completionMessages.OrderBy(m => m.Key))
                        Console.WriteLine("  {0}  Total: {1}, Failed: {2}, Skipped: {3}",
                                          message.Key.PadRight(longestAssemblyName),
                                          message.Value.Total.ToString().PadLeft(longestTotal),
                                          message.Value.Failed.ToString().PadLeft(longestFailed),
                                          message.Value.Skipped.ToString().PadLeft(longestSkipped));
                }
            }

            Directory.SetCurrentDirectory(originalWorkingFolder);

            if (needsXml)
            {
                //if (Xml != null)
                //    assembliesElement.Save(Xml.GetMetadata("FullPath"));

                //if (XmlV1 != null)
                //    Transform("xUnit1.xslt", assembliesElement, XmlV1);

                //if (Html != null)
                //    Transform("HTML.xslt", assembliesElement, Html);
            }

            return completionMessages.Values.Sum(summary => summary.Failed);
        }

        static XmlTestExecutionVisitor CreateVisitor(XElement assemblyElement, bool teamCity, bool silent)
        {
            if (teamCity)
                return new TeamCityVisitor(assemblyElement, () => cancel);

            return new StandardOutputVisitor(assemblyElement, !silent, () => cancel, completionMessages);
        }

        static XElement ExecuteAssembly(XunitProjectAssembly assembly, bool needsXml, bool teamCity, bool silent)
        {
            if (cancel)
                return null;

            var assemblyElement = needsXml ? new XElement("assembly") : null;

            try
            {
                using (var controller = new XunitFrontController(assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy))
                {
                    var discoveryVisitor = new TestDiscoveryVisitor();
                    controller.Find(includeSourceInformation: false, messageSink: discoveryVisitor);
                    discoveryVisitor.Finished.WaitOne();

                    var resultsVisitor = CreateVisitor(assemblyElement, teamCity, silent);
                    controller.Run(discoveryVisitor.TestCases, resultsVisitor);
                    resultsVisitor.Finished.WaitOne();
                }

            }
            catch (Exception ex)
            {
                Exception e = ex;

                while (e != null)
                {
                    Console.WriteLine(e.GetType().FullName + ": " + e.Message);

                    foreach (string stackLine in e.StackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        Console.WriteLine(stackLine);

                    e = e.InnerException;
                }
            }

            return assemblyElement;
        }
    }
}