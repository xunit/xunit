using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Xunit.ConsoleClient
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();
        readonly string executablePath;

        protected CommandLine(string[] args)
        {
            for (int i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            executablePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
            ParallelizeAssemblies = false;
            ParallelizeTestCollections = true;
            Project = Parse();
        }

        public int MaxParallelThreads { get; set; }

        public XunitProject Project { get; protected set; }

        public bool ParallelizeAssemblies { get; protected set; }

        public bool ParallelizeTestCollections { get; set; }

        public bool Silent { get; protected set; }

        public bool TeamCity { get; protected set; }

        public bool Wait { get; protected set; }

        static XunitProject GetSingleAssemblyProject(string assemblyFile, string configFile)
        {
            return new XunitProject
            {
                new XunitProjectAssembly
                {
                    AssemblyFilename = assemblyFile,
                    ConfigFilename = configFile,
                    ShadowCopy = true
                }
            };
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

        protected virtual XunitProject Parse()
        {
            return Parse(fileName => File.Exists(fileName));
        }

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var transforms = new Dictionary<string, string>();

            var filename = arguments.Pop();
            if (!fileExists(filename))
                throw new ArgumentException(String.Format("file not found: {0}", filename));

            string configFile = null;
            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-"))
            {
                configFile = arguments.Pop();

                if (!fileExists(configFile))
                    throw new ArgumentException(String.Format("config file not found: {0}", configFile));
            }

            var project = GetSingleAssemblyProject(filename, configFile);

            while (arguments.Count > 0)
            {
                var option = PopOption(arguments);
                var optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("-"))
                    throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));

                if (optionName == "-wait")
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
                else if (optionName == "-silent")
                {
                    GuardNoOptionValue(option);
                    Silent = true;
                }
                else if (optionName == "-teamcity")
                {
                    GuardNoOptionValue(option);
                    TeamCity = true;
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

                    string name = pieces[0];
                    string value = pieces[1];
                    project.Filters.IncludedTraits.Add(name, value);
                }
                else if (optionName == "-notrait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -notrait");

                    var pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for -notrait (should be \"name=value\")");

                    string name = pieces[0];
                    string value = pieces[1];
                    project.Filters.ExcludedTraits.Add(name, value);
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
            string option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("-"))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}
