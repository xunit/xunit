using System;
using System.Collections.Generic;
using System.IO;

namespace Xunit.ConsoleClient
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();

        protected CommandLine(string[] args, Predicate<string> fileExists = null)
        {
            if (fileExists == null)
                fileExists = fileName => File.Exists(fileName);

            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
            AppVeyor = Environment.GetEnvironmentVariable("APPVEYOR_API_URL") != null;
            Project = Parse(fileExists);
        }

        public bool AppVeyor { get; protected set; }

        public bool Debug { get; protected set; }

        public int? MaxParallelThreads { get; set; }

        public bool NoLogo { get; protected set; }

        public XunitProject Project { get; protected set; }

        public bool? ParallelizeAssemblies { get; protected set; }

        public bool? ParallelizeTestCollections { get; set; }

        public bool Quiet { get; protected set; }

        public bool TeamCity { get; protected set; }

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
                throw new ArgumentException(String.Format("error: unknown command line option: {0}", option.Value));
        }

        public static CommandLine Parse(params string[] args)
        {
            return new CommandLine(args);
        }

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var assemblies = new List<Tuple<string, string>>();

            while (arguments.Count > 0)
            {
                if (arguments.Peek().StartsWith("-"))
                    break;

                var assemblyFile = arguments.Pop();
                if (assemblyFile.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(String.Format("expecting assembly, got config file: {0}", assemblyFile));
                if (!fileExists(assemblyFile))
                    throw new ArgumentException(String.Format("file not found: {0}", assemblyFile));

                string configFile = null;
                if (arguments.Count > 0)
                {
                    var value = arguments.Peek();
                    if (!value.StartsWith("-") && value.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
                    {
                        configFile = arguments.Pop();
                        if (!fileExists(configFile))
                            throw new ArgumentException(String.Format("config file not found: {0}", configFile));
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

                if (!optionName.StartsWith("-"))
                    throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));

                if (optionName == "-nologo")
                {
                    GuardNoOptionValue(option);
                    NoLogo = true;
                }
                else if (optionName == "-quiet")
                {
                    GuardNoOptionValue(option);
                    Quiet = true;
                }
                else if (optionName == "-debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
                }
                else if (optionName == "-wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "-maxthreads")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -maxthreads");

                    int threadValue;
                    if (!Int32.TryParse(option.Value, out threadValue) || threadValue < 0)
                        throw new ArgumentException("incorrect argument value for -maxthreads");

                    MaxParallelThreads = threadValue;
                }
                else if (optionName == "-parallel")
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

                        case ParallelismOption.none:
                        default:
                            ParallelizeAssemblies = false;
                            ParallelizeTestCollections = false;
                            break;
                    }
                }
                else if (optionName == "-teamcity")
                {
                    GuardNoOptionValue(option);
                    TeamCity = true;
                }
                else if (optionName == "-appveyor")
                {
                    GuardNoOptionValue(option);
                    AppVeyor = true;
                }
                else if (optionName == "-noshadow")
                {
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.ShadowCopy = false;
                }
                else if (optionName == "-trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -trait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -trait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.IncludedTraits.Add(name, value);
                }
                else if (optionName == "-notrait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -notrait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

                    var name = pieces[0];
                    var value = pieces[1];
                    project.Filters.ExcludedTraits.Add(name, value);
                }
                else if (optionName == "-class")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -class");

                    project.Filters.IncludedClasses.Add(option.Value);
                }
                else if (optionName == "-method")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -method");

                    project.Filters.IncludedMethods.Add(option.Value);
                }
                else
                {
                    if (option.Value == null)
                        throw new ArgumentException(String.Format("missing filename for {0}", option.Key));

                    project.Output.Add(optionName.Substring(1), option.Value);
                }
            }

            return project;
        }

        static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            var option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-"))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}
