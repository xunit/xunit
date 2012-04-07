using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class xunitproject : Task
    {
        public xunitproject()
        {
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
        }

        [Output]
        public int ExitCode { get; private set; }

        [Required]
        public ITaskItem ProjectFile { get; set; }

        public bool TeamCity { get; set; }

        public bool Verbose { get; set; }

        public override bool Execute()
        {
            RemotingUtility.CleanUpRegisteredChannels();

            try
            {
                ExitCode = 0;

                string projectFilename = ProjectFile.GetMetadata("FullPath");
                XunitProject project = XunitProject.Load(projectFilename);
                IRunnerLogger logger =
                    TeamCity ? (IRunnerLogger)new TeamCityLogger(Log) :
                    Verbose ? new VerboseLogger(Log) :
                    new StandardLogger(Log);

                Log.LogMessage(MessageImportance.High, "xUnit.net MSBuild runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);

                foreach (XunitProjectAssembly assembly in project.Assemblies)
                {
                    using (Stream htmlStream = ResourceStream("HTML.xslt"))
                    using (Stream nunitStream = ResourceStream("NUnitXml.xslt"))
                    using (ExecutorWrapper wrapper = new ExecutorWrapper(assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy))
                    {
                        Log.LogMessage(MessageImportance.High, "  Test assembly: {0}", assembly.AssemblyFilename);
                        Log.LogMessage(MessageImportance.High, "  xunit.dll version: {0}", wrapper.XunitVersion);

                        List<IResultXmlTransform> transforms = new List<IResultXmlTransform>();

                        foreach (KeyValuePair<string, string> kvp in assembly.Output)
                        {
                            switch (kvp.Key.ToLowerInvariant())
                            {
                                case "xml":
                                    transforms.Add(new NullTransformer(kvp.Value));
                                    break;

                                case "html":
                                    transforms.Add(new XslStreamTransformer(htmlStream, kvp.Value));
                                    break;

                                case "nunit":
                                    transforms.Add(new XslStreamTransformer(nunitStream, kvp.Value));
                                    break;

                                default:
                                    Log.LogWarning("Unknown output type: '{0}'", kvp.Key);
                                    break;
                            }
                        }

                        TestRunner runner = new TestRunner(wrapper, logger);
                        if (runner.RunAssembly(transforms) == TestRunnerResult.Failed)
                            ExitCode = -1;
                    }
                }

                StandardLogger stdLogger = logger as StandardLogger;
                if (stdLogger != null)
                {
                    Log.LogMessage(MessageImportance.High,
                                   "TOTAL Tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                                   stdLogger.Total,
                                   stdLogger.Failed,
                                   stdLogger.Skipped,
                                   stdLogger.Time.ToString("0.000"));
                }
            }
            catch (Exception ex)
            {
                Exception e = ex;

                while (e != null)
                {
                    Log.LogError(e.GetType().FullName + ": " + e.Message);

                    foreach (string stackLine in e.StackTrace.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                        Log.LogError(stackLine);

                    e = e.InnerException;
                }

                ExitCode = -1;
            }

            return ExitCode == 0;
        }

        static Stream ResourceStream(string xmlResourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("Xunit.Runner.MSBuild." + xmlResourceName);
        }
    }
}
