using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Xunit.ConsoleClient
{
    public class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Console.WriteLine("xUnit.net console test runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);
            Console.WriteLine("Copyright (C) 2013 Outercurve Foundation.");

            if (args.Length == 0 || args[0] == "/?")
            {
                PrintUsage();
                return -1;
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                CommandLine commandLine = CommandLine.Parse(args);

                int failCount = RunProject(commandLine.Project, commandLine.TeamCity, commandLine.Silent);

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
                return -1;
            }
            catch (BadImageFormatException ex)
            {
                Console.WriteLine();
                Console.WriteLine("{0}", ex.Message);
                return -1;
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
            Console.WriteLine("  /silent                : do not output running test count");
            Console.WriteLine("  /teamcity              : forces TeamCity mode (normally auto-detected)");
            Console.WriteLine("  /wait                  : wait for input after completion");
            Console.WriteLine("  /trait \"name=value\"    : only run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an OR operation");
            Console.WriteLine("  /-trait \"name=value\"   : do not run tests with matching name/value traits");
            Console.WriteLine("                         : if specified more than once, acts as an AND operation");
            Console.WriteLine();
            Console.WriteLine("Valid options for assemblies only:");
            Console.WriteLine("  /noshadow              : do not shadow copy assemblies");
            Console.WriteLine("  /xml <filename>        : output results to Xunit-style XML file");

            foreach (TransformConfigurationElement transform in TransformFactory.GetInstalledTransforms())
            {
                string commandLine = "/" + transform.CommandLine + " <filename>";
                commandLine = commandLine.PadRight(22).Substring(0, 22);

                Console.WriteLine("  {0} : {1}", commandLine, transform.Description);
            }
        }

        static int RunProject(XunitProject project, bool teamcity, bool silent)
        {
            int totalAssemblies = 0;
            int totalTests = 0;
            int totalFailures = 0;
            int totalSkips = 0;
            double totalTime = 0;

            var mate = new MultiAssemblyTestEnvironment();

            foreach (XunitProjectAssembly assembly in project.Assemblies)
            {
                TestAssembly testAssembly = mate.Load(assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy);
                List<IResultXmlTransform> transforms = TransformFactory.GetAssemblyTransforms(assembly);

                Console.WriteLine();
                Console.WriteLine("xunit.dll:     Version {0}", testAssembly.XunitVersion);
                Console.WriteLine("Test assembly: {0}", testAssembly.AssemblyFilename);
                Console.WriteLine();

                try
                {
                    var methods = new List<TestMethod>(testAssembly.EnumerateTestMethods(project.Filters.Filter));
                    if (methods.Count == 0)
                    {
                        Console.WriteLine("Skipping assembly (no tests match the specified filter).");
                        continue;
                    }

                    var callback =
                        teamcity ? (RunnerCallback)new TeamCityRunnerCallback()
                                 : new StandardRunnerCallback(silent, methods.Count);
                    var assemblyXml = testAssembly.Run(methods, callback);

                    ++totalAssemblies;
                    totalTests += callback.TotalTests;
                    totalFailures += callback.TotalFailures;
                    totalSkips += callback.TotalSkips;
                    totalTime += callback.TotalTime;

                    foreach (var transform in transforms)
                        transform.Transform(assemblyXml);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }

                mate.Unload(testAssembly);
            }

            if (!teamcity && totalAssemblies > 1)
            {
                Console.WriteLine();
                Console.WriteLine("=== {0} total, {1} failed, {2} skipped, took {3} seconds ===",
                                   totalTests, totalFailures, totalSkips, totalTime.ToString("0.000", CultureInfo.InvariantCulture));
            }

            return totalFailures;
        }
    }
}