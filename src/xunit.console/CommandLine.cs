using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Xunit.ConsoleClient
{
    public class CommandLine
    {
        Stack<string> arguments = new Stack<string>();
        string executablePath;

        protected CommandLine(string[] args)
        {
            for (int i = args.Length - 1; i >= 0; i--)
                arguments.Push(args[i]);

            executablePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
            Project = Parse();
        }

        public XunitProject Project { get; protected set; }

        public bool Silent { get; protected set; }

        public bool TeamCity { get; protected set; }

        public bool Wait { get; protected set; }

        protected virtual XunitProject GetMultiAssemblyProject(string filename)
        {
            return XunitProject.Load(filename);
        }

        static XunitProject GetSingleAssemblyProject(string assemblyFile, string configFile)
        {
            XunitProject project = new XunitProject();
            project.AddAssembly(
                new XunitProjectAssembly
                {
                    AssemblyFilename = assemblyFile,
                    ConfigFilename = configFile,
                    ShadowCopy = true
                }
            );
            return project;
        }

        static void GuardNoOptionValue(KeyValuePair<string, string> option)
        {
            if (option.Value != null)
                throw new ArgumentException(String.Format("error: unknown command line option: {0}", option.Value));
        }

        static void GuardNoProjectFile(bool passedProjectFile, KeyValuePair<string, string> option)
        {
            if (passedProjectFile)
                throw new ArgumentException(String.Format("the {0} command line option isn't valid for .xunit projects", option.Key));
        }

        public static bool IsProjectFilename(string filename)
        {
            return Path.GetExtension(filename).Equals(".xunit", StringComparison.OrdinalIgnoreCase);
        }

        public static CommandLine Parse(string[] args)
        {
            return new CommandLine(args);
        }

        protected virtual XunitProject Parse()
        {
            return Parse(fileName => File.Exists(fileName));
        }

        protected XunitProject Parse(Predicate<string> fileExists)
        {
            Dictionary<string, string> transforms = new Dictionary<string, string>();
            bool passedProjectFile = false;
            XunitProject project = null;

            string filename = arguments.Pop();
            if (!fileExists(filename))
                throw new ArgumentException(String.Format("file not found: {0}", filename));

            if (IsProjectFilename(filename))
            {
                project = GetMultiAssemblyProject(filename);
                passedProjectFile = true;
            }
            else
            {
                string configFile = null;

                if (arguments.Count > 0 && !arguments.Peek().StartsWith("/"))
                {
                    configFile = arguments.Pop();

                    if (!fileExists(configFile))
                        throw new ArgumentException(String.Format("config file not found: {0}", configFile));
                }

                project = GetSingleAssemblyProject(filename, configFile);
            }

            while (arguments.Count > 0)
            {
                KeyValuePair<string, string> option = PopOption(arguments);
                string optionName = option.Key.ToLowerInvariant();

                if (!optionName.StartsWith("/"))
                    throw new ArgumentException(String.Format("unknown command line option: {0}", option.Key));

                if (optionName == "/wait")
                {
                    GuardNoOptionValue(option);
                    Wait = true;
                }
                else if (optionName == "/silent")
                {
                    GuardNoOptionValue(option);
                    Silent = true;
                }
                else if (optionName == "/teamcity")
                {
                    GuardNoOptionValue(option);
                    TeamCity = true;
                }
                else if (optionName == "/noshadow")
                {
                    GuardNoProjectFile(passedProjectFile, option);
                    GuardNoOptionValue(option);
                    foreach (var assembly in project.Assemblies)
                        assembly.ShadowCopy = false;
                }
                else if (optionName == "/trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for /trait");

                    string[] pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for /trait (should be \"name=value\")");

                    string name = pieces[0];
                    string value = pieces[1];
                    project.Filters.IncludedTraits.AddValue(name, value);
                }
                else if (optionName == "/-trait")
                {
                    if (option.Value == null)
                        throw new ArgumentException("missing argument for /-trait");

                    string[] pieces = option.Value.Split('=');
                    if (pieces.Length != 2 || String.IsNullOrEmpty(pieces[0]) || String.IsNullOrEmpty(pieces[1]))
                        throw new ArgumentException("incorrect argument format for /-trait (should be \"name=value\")");

                    string name = pieces[0];
                    string value = pieces[1];
                    project.Filters.ExcludedTraits.AddValue(name, value);
                }
                else
                {
                    GuardNoProjectFile(passedProjectFile, option);

                    if (option.Value == null)
                        throw new ArgumentException(String.Format("missing filename for {0}", option.Key));

                    foreach (var assembly in project.Assemblies)
                        assembly.Output.Add(optionName.Substring(1), option.Value);
                }
            }

            return project;
        }

        static KeyValuePair<string, string> PopOption(Stack<string> arguments)
        {
            string option = arguments.Pop();
            string value = null;

            if (arguments.Count > 0 && !arguments.Peek().StartsWith("/"))
                value = arguments.Pop();

            return new KeyValuePair<string, string>(option, value);
        }
    }
}
