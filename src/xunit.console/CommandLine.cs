using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Xunit.ConsoleClient
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();
        readonly IReadOnlyList<IRunnerReporter> reporters;

        protected CommandLine(IReadOnlyList<IRunnerReporter> reporters, string[] args, Predicate<string> fileExists = null)
        {
            this.reporters = reporters;

            if (fileExists == null)
                fileExists = File.Exists;

            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            Project = Parse(fileExists);
            Reporter = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) ?? Reporter ?? new DefaultRunnerReporter();
        }

        public bool Debug { get; protected set; }

        public bool DiagnosticMessages { get; protected set; }

        public int? MaxParallelThreads { get; set; }

        public bool? NoAppDomain { get; protected set; }

        public bool NoColor { get; protected set; }

        public bool NoLogo { get; protected set; }

        public XunitProject Project { get; protected set; }

        public bool? ParallelizeAssemblies { get; protected set; }

        public bool? ParallelizeTestCollections { get; set; }

        public IRunnerReporter Reporter { get; protected set; }

        public bool Serialize { get; protected set; }

        public bool Wait { get; protected set; }

        static XunitProject GetProjectFile(List<Tuple<string, string>> assemblies)
        {
            var result = new XunitProject();

            foreach (var assembly in assemblies)
                result.Add(new XunitProjectAssembly
                {
                    AssemblyFilename = Path.GetFullPath(assembly.Item1),
                    ConfigFilename = assembly.Item2 != null ? Path.GetFullPath(assembly.Item2) : null,
                    ShadowCopy = true
                });

            return result;
        }

        static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException($"error: unknown command line option: {option.Value}");
        }

        static bool IsConfigFile(string fileName)
        {
            return fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        public static CommandLine Parse(IReadOnlyList<IRunnerReporter> reporters, params string[] args)
        {
            return new CommandLine(reporters, args);
        }

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var assemblies = new List<Tuple<string, string>>();

            while (arguments.Count > 0)
            {
                if (arguments.Peek().StartsWith("-", StringComparison.Ordinal))
                    break;

                var assemblyFile = arguments.Pop();
                if (IsConfigFile(assemblyFile))
                    throw new ArgumentException($"expecting assembly, got config file: {assemblyFile}");
                if (!fileExists(assemblyFile))
                    throw new ArgumentException($"file not found: {assemblyFile}");

                string configFile = null;
                if (arguments.Count > 0)
                {
                    var value = arguments.Peek();
                    if (!value.StartsWith("-", StringComparison.Ordinal) && IsConfigFile(value))
                    {
                        configFile = arguments.Pop();
                        if (!fileExists(configFile))
                            throw new ArgumentException($"config file not found: {configFile}");
                    }
                }

                assemblies.Add(Tuple.Create(assemblyFile, configFile));
            }

            if (assemblies.Count == 0)
                throw new ArgumentException("must specify at least one assembly");

            var project = GetProjectFile(assemblies);

            while (arguments.Count > 0)
            {
                var option = PopOption(arguments);
                var optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("-", StringComparison.Ordinal))
                    throw new ArgumentException($"unknown command line option: {option.Key}");

                optionName = optionName.Substring(1);

                if (optionName == "nologo")
                {
                    GuardNoOptionValue(option);
                    NoLogo = true;
                }
                else if (optionName == "nocolor")
                {
                    GuardNoOptionValue(option);
                    NoColor = true;
                }
                else if (optionName == "noappdomain")
                {
                    GuardNoOptionValue(option);
                    NoAppDomain = true;
                }
                else if (optionName == "debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
                }
                else if (optionName == "serialize")
                {
                    GuardNoOptionValue(option);
                    Serialize = true;
                }
                else if (optionName == "wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "diagnostics")
                {
                    GuardNoOptionValue(option);
                    DiagnosticMessages = true;
                }
                else if (optionName == "maxthreads")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -maxthreads");

                    switch (option.Value)
                    {
                        case "default":
                            MaxParallelThreads = 0;
                            break;

                        case "unlimited":
                            MaxParallelThreads = -1;
                            break;

                        default:
                            int threadValue;
                            if (!int.TryParse(option.Value, out threadValue) || threadValue < 1)
                                throw new ArgumentException("incorrect argument value for -maxthreads (must be 'default', 'unlimited', or a positive number)");

                            MaxParallelThreads = threadValue;
                            break;
                    }
                }
                else if (optionName == "parallel")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -parallel");

                    ParallelismOption parallelismOption;
                    if (!Enum.TryParse<ParallelismOption>(option.Value, out parallelismOption))
                        throw new ArgumentException("incorrect argument value for -parallel");

                    switch (parallelismOption)
                    {
                        case ParallelismOption.all:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = true;
                            break;

                        case ParallelismOption.assemblies:
                            ParallelizeAssemblies = true;
                            ParallelizeTestCollections = false;
                            break;

                        case ParallelismOption.collections:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = true;
                            break;

                        default:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = false;
                            break;
                    }
                }
                else if (optionName == "noshadow")
                {
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.ShadowCopy = false;
                }
                else if (optionName == "trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -trait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.IncludedTraits.Add(name, value);
                }
                else if (optionName == "notrait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -notrait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || string.IsNullOrEmpty(pieces[0]) || string.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.ExcludedTraits.Add(name, value);
                }
                else if (optionName == "class")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -class");

                    project.Filters.IncludedClasses.Add(option.Value);
                }
                else if (optionName == "method")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -method");

                    project.Filters.IncludedMethods.Add(option.Value);
                }
                else
                {
                    // Might be a reporter...
                    var reporter = reporters.FirstOrDefault(r => string.Equals(r.RunnerSwitch, optionName, StringComparison.OrdinalIgnoreCase));
                    if (reporter != null)
                    {
                        GuardNoOptionValue(option);
                        if (Reporter != null)
                            throw new ArgumentException("only one reporter is allowed");

                        Reporter = reporter;
                    }
                    // ...or an result output file
                    else
                    {
                        if (option.Value == null)
                            throw new ArgumentException($"missing filename for {option.Key}");

                        project.Output.Add(optionName, option.Value);
                    }
                }
            }

            return project;
        }

        static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            var option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}
