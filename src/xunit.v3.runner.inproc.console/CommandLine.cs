using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit.Runner.Common;

namespace Xunit.Runner.InProc.SystemConsole
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();
        readonly string assemblyFileName;
        readonly List<string> unknownOptions = new List<string>();

        protected CommandLine(string assemblyFileName,
                              string[] args,
                              Predicate<string> fileExists = null)
        {
            this.assemblyFileName = assemblyFileName;

            if (fileExists == null)
                fileExists = File.Exists;

            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            Project = Parse(fileExists);
        }

        public bool Debug { get; protected set; }

        public bool DiagnosticMessages { get; protected set; }

        public bool InternalDiagnosticMessages { get; protected set; }

        public bool FailSkips { get; protected set; }

        public int? MaxParallelThreads { get; set; }

        public bool NoAutoReporters { get; protected set; }

        public bool NoColor { get; protected set; }

        public bool NoLogo { get; protected set; }

        public bool Pause { get; protected set; }

        public XunitProject Project { get; protected set; }

        public bool? ParallelizeTestCollections { get; set; }

        public bool StopOnFail { get; protected set; }

        public bool Wait { get; protected set; }

        public IRunnerReporter ChooseReporter(IReadOnlyList<IRunnerReporter> reporters)
        {
            var result = default(IRunnerReporter);

            foreach (var unknownOption in unknownOptions)
            {
                var reporter = reporters.FirstOrDefault(r => r.RunnerSwitch == unknownOption) ?? throw new ArgumentException($"unknown option: -{unknownOption}");

                if (result != null)
                    throw new ArgumentException("only one reporter is allowed");

                result = reporter;
            }

            if (!NoAutoReporters)
                result = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled) ?? result;

            return result ?? new DefaultRunnerReporterWithTypes();
        }

        protected virtual string GetFullPath(string fileName)
        {
            return Path.GetFullPath(fileName);
        }

        XunitProject GetProjectFile(string assemblyFileName, string configFileName)
            => new XunitProject
            {
                new XunitProjectAssembly
                {
                    AssemblyFilename = assemblyFileName,
                    ConfigFilename = configFileName != null ? GetFullPath(configFileName) : null
                }
            };

        static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException($"error: unknown command line option: {option.Value}");
        }

        static bool IsConfigFile(string fileName)
            => fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

        public static CommandLine Parse(string assemblyFileName, params string[] args)
            => new CommandLine(assemblyFileName, args);

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var configFileName = default(string);

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-", StringComparison.Ordinal))
            {
                configFileName = arguments.Pop();
                if (!IsConfigFile(configFileName))
                    throw new ArgumentException($"expecting config file, got: {configFileName}");
                if (!fileExists(configFileName))
                    throw new ArgumentException($"config file not found: {configFileName}");
            }

            var project = GetProjectFile(assemblyFileName, configFileName);

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
                else if (optionName == "failskips")
                {
                    GuardNoOptionValue(option);
                    FailSkips = true;
                }
                else if (optionName == "stoponfail")
                {
                    GuardNoOptionValue(option);
                    StopOnFail = true;
                }
                else if (optionName == "nocolor")
                {
                    GuardNoOptionValue(option);
                    NoColor = true;
                }
                else if (optionName == "noautoreporters")
                {
                    GuardNoOptionValue(option);
                    NoAutoReporters = true;
                }
                else if (optionName == "pause")
                {
                    GuardNoOptionValue(option);
                    Pause = true;
                }
                else if (optionName == "debug")
                {
                    GuardNoOptionValue(option);
                    Debug = true;
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
                else if (optionName == "internaldiagnostics")
                {
                    GuardNoOptionValue(option);
                    InternalDiagnosticMessages = true;
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

                    if (!Enum.TryParse(option.Value, out ParallelismOption parallelismOption))
                        throw new ArgumentException("incorrect argument value for -parallel");

                    ParallelizeTestCollections = parallelismOption switch
                    {
                        ParallelismOption.collections => true,
                        _ => false,
                    };
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
                else if (optionName == "noclass")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -noclass");

                    project.Filters.ExcludedClasses.Add(option.Value);
                }
                else if (optionName == "method")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -method");

                    project.Filters.IncludedMethods.Add(option.Value);
                }
                else if (optionName == "nomethod")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -nomethod");

                    project.Filters.ExcludedMethods.Add(option.Value);
                }
                else if (optionName == "namespace")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -namespace");

                    project.Filters.IncludedNamespaces.Add(option.Value);
                }
                else if (optionName == "nonamespace")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -nonamespace");

                    project.Filters.ExcludedNamespaces.Add(option.Value);
                }
                else
                {
                    // Might be a result output file...
                    if (TransformFactory.AvailableTransforms.Any(t => t.ID.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (option.Value == null)
                            throw new ArgumentException($"missing filename for {option.Key}");

                        EnsurePathExists(option.Value);

                        project.Output.Add(optionName, option.Value);
                    }
                    // ...or it might be a reporter (we won't know until later)
                    else
                    {
                        GuardNoOptionValue(option);
                        unknownOptions.Add(optionName);
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

        static void EnsurePathExists(string path)
        {
            var directory = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directory))
                return;

            Directory.CreateDirectory(directory);
        }
    }
}
