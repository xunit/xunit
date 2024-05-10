using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Xunit.ConsoleClient
{
    public class CommandLine
    {
        readonly Stack<string> arguments = new Stack<string>();
        readonly List<string> unknownOptions = new List<string>();

        protected CommandLine(string[] args, Predicate<string> fileExists = null)
        {
            if (fileExists == null)
                fileExists = File.Exists;

            for (var i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            Project = Parse(fileExists);
        }

        public AppDomainSupport? AppDomains { get; protected set; }

        public bool Debug { get; protected set; }

        public bool DiagnosticMessages { get; protected set; }

        public bool InternalDiagnosticMessages { get; protected set; }

        public bool FailSkips { get; protected set; }

        public int? MaxParallelThreads { get; set; }

        public bool NoAutoReporters { get; protected set; }

        public bool NoColor { get; protected set; }

        public bool NoLogo { get; protected set; }

#if DEBUG
        public bool Pause { get; protected set; }
#endif

        public XunitProject Project { get; protected set; }

        public ParallelAlgorithm? ParallelAlgorithm { get; protected set; }

        public bool? ParallelizeAssemblies { get; protected set; }

        public bool? ParallelizeTestCollections { get; set; }

        public bool Serialize { get; protected set; }

        public bool ShowLiveOutput { get; protected set; }

        public bool StopOnFail { get; protected set; }

        public bool UseAnsiColor { get; protected set; }

        public bool Wait { get; protected set; }

        public IRunnerReporter ChooseReporter(IReadOnlyList<IRunnerReporter> reporters)
        {
            var result = default(IRunnerReporter);

            foreach (var unknownOption in unknownOptions)
            {
                var reporter = reporters.FirstOrDefault(r => r.RunnerSwitch == unknownOption) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "unknown option: -{0}", unknownOption));

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

        XunitProject GetProjectFile(List<Tuple<string, string>> assemblies)
        {
            var result = new XunitProject();

            foreach (var assembly in assemblies)
                result.Add(new XunitProjectAssembly
                {
                    AssemblyFilename = GetFullPath(assembly.Item1),
                    ConfigFilename = assembly.Item2 != null ? GetFullPath(assembly.Item2) : null,
                });

            return result;
        }

        static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "error: unknown command line option: {0}", option.Value));
        }

        static bool IsConfigFile(string fileName)
        {
            return fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        public static CommandLine Parse(params string[] args)
            => new CommandLine(args);

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            var assemblies = new List<Tuple<string, string>>();

            while (arguments.Count > 0)
            {
                if (arguments.Peek().StartsWith("-", StringComparison.Ordinal))
                    break;

                var assemblyFile = arguments.Pop();
                if (IsConfigFile(assemblyFile))
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "expecting assembly, got config file: {0}", assemblyFile));
                if (!fileExists(assemblyFile))
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "file not found: {0}", assemblyFile));

                string configFile = null;
                if (arguments.Count > 0)
                {
                    var value = arguments.Peek();
                    if (!value.StartsWith("-", StringComparison.Ordinal) && IsConfigFile(value))
                    {
                        configFile = arguments.Pop();
                        if (!fileExists(configFile))
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "config file not found: {0}", configFile));
                    }
                }

                assemblies.Add(Tuple.Create(assemblyFile, configFile));
            }

            var project = GetProjectFile(assemblies);

            while (arguments.Count > 0)
            {
                var option = PopOption(arguments);
                var optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("-", StringComparison.Ordinal))
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "unknown command line option: {0}", option.Key));

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
                else if (optionName == "showliveoutput")
                {
                    GuardNoOptionValue(option);
                    ShowLiveOutput = true;
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
                    TransformFactory.NoErrorColoring = NoColor;
                }
                else if (optionName == "noappdomain")    // Here for historical reasons
                {
                    GuardNoOptionValue(option);
                    AppDomains = AppDomainSupport.Denied;
                }
                else if (optionName == "noautoreporters")
                {
                    GuardNoOptionValue(option);
                    NoAutoReporters = true;
                }
#if DEBUG
                else if (optionName == "pause")
                {
                    GuardNoOptionValue(option);
                    Pause = true;
                }
#endif
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
                else if (optionName == "internaldiagnostics")
                {
                    GuardNoOptionValue(option);
                    InternalDiagnosticMessages = true;
                }
                else if (optionName == "appdomains")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -appdomains");

                    switch (option.Value)
                    {
                        case "ifavailable":
                            AppDomains = AppDomainSupport.IfAvailable;
                            break;

                        case "required":
#if NETFRAMEWORK
                            // We don't want to throw here on .NET Core, because the user may be specifying a value
                            // via "dotnet xunit" that is only compatible with some target frameworks.
                            AppDomains = AppDomainSupport.Required;
#endif
                            break;

                        case "denied":
                            AppDomains = AppDomainSupport.Denied;
                            break;

                        default:
                            throw new ArgumentException("incorrect argument value for -appdomains (must be 'ifavailable', 'required', or 'denied')");

                    }
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
                            var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(option.Value);
                            // Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
                            // If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
                            // from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
                            if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier))
                                MaxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
                            else if (int.TryParse(option.Value, out var threadValue) && threadValue > 0)
                                MaxParallelThreads = threadValue;
                            else
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "incorrect argument value for -maxthreads (must be 'default', 'unlimited', a positive number, or a multiplier in the form of '{0}x')", 0.0m));

                            break;
                    }
                }
                else if (optionName == "parallel")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -parallel");

                    if (!Enum.TryParse(option.Value, out ParallelismOption parallelismOption))
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
                else if (optionName == "parallelalgorithm")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for -parallelAlgorithm");

                    if (!Enum.TryParse(option.Value, ignoreCase: true, out ParallelAlgorithm parallelAlgorithm))
                        throw new ArgumentException("incorrect argument value for -parallelAlgorithm");

                    ParallelAlgorithm = parallelAlgorithm;
                }
                else if (optionName == "noshadow")
                {
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.Configuration.ShadowCopy = false;
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
                else if (optionName == "useansicolor")
                {
                    GuardNoOptionValue(option);
                    UseAnsiColor = true;
                }
                else
                {
                    // Might be a result output file...
                    if (TransformFactory.AvailableTransforms.Any(t => t.CommandLine.Equals(optionName, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (option.Value == null)
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "missing filename for {0}", option.Key));

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
