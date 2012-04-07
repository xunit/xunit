using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class xunit : Task
    {
        public xunit()
        {
            ShadowCopy = true;
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
        }

        public ITaskItem Assembly { get; set; }

        public ITaskItem[] Assemblies { get; set; }

        public string ConfigFile { get; set; }

        [Output]
        public int ExitCode { get; private set; }

        public ITaskItem Html { get; set; }

        public bool ShadowCopy { get; set; }

        public bool TeamCity { get; set; }

        public bool Verbose { get; set; }

        public string WorkingFolder { get; set; }

        public ITaskItem Xml { get; set; }

        public override bool Execute()
        {
            RemotingUtility.CleanUpRegisteredChannels();

            if (!GuardParameters())
            {
                ExitCode = -2;
                return false;
            }

            StringBuilder assembliesNode = new StringBuilder();
            assembliesNode.Append("<assemblies>");

            if (WorkingFolder != null)
                Directory.SetCurrentDirectory(WorkingFolder);

            ExitCode = 0;
            Log.LogMessage(MessageImportance.High, "xUnit.net MSBuild runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);

            IRunnerLogger logger =
                TeamCity ? (IRunnerLogger)new TeamCityLogger(Log) :
                 Verbose ? new VerboseLogger(Log) :
                           new StandardLogger(Log);

            if (Assembly != null)
            {
                string assemblyFilename = Assembly.GetMetadata("FullPath");
                assembliesNode.Append(ExecuteAssembly(assemblyFilename, ConfigFile, logger));
            }
            else
            {
                foreach (ITaskItem assembly in Assemblies)
                {
                    string assemblyFilename = assembly.GetMetadata("FullPath");
                    string configFilename = assembly.GetMetadata("ConfigFile");
                    if (configFilename.Length == 0)
                        configFilename = null;

                    assembliesNode.Append(ExecuteAssembly(assemblyFilename, configFilename, logger));
                }
            }

            assembliesNode.Append("</assemblies>");

            string fullXml = assembliesNode.ToString();

            if (Xml != null)
                new NullTransformer(Xml.GetMetadata("FullPath")).Transform(fullXml);

            if (Html != null)
                using (Stream htmlStream = ResourceStream("HTML.xslt"))
                    new XslStreamTransformer(htmlStream, Html.GetMetadata("FullPath")).Transform(fullXml);

            return ExitCode == 0;
        }

        string ExecuteAssembly(string assemblyFilename, string configFilename, IRunnerLogger logger)
        {
            try
            {
                using (ExecutorWrapper wrapper = new ExecutorWrapper(assemblyFilename, configFilename, ShadowCopy))
                {
                    Log.LogMessage(MessageImportance.High, "xunit.dll:     Version {0}", wrapper.XunitVersion);
                    Log.LogMessage(MessageImportance.High, "Test assembly: {0}", assemblyFilename);

                    XmlTestRunner runner = new XmlTestRunner(wrapper, logger);
                    if (runner.RunAssembly() == TestRunnerResult.Failed)
                        ExitCode = -1;

                    return runner.Xml;
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
                return "";
            }
        }

        static Stream ResourceStream(string xmlResourceName)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Xunit.Runner.MSBuild." + xmlResourceName);
        }

        bool GuardParameters()
        {
            if ((Assembly == null && Assemblies == null) || (Assembly != null && Assemblies != null))
            {
                Log.LogError("The xunit task requires either Assembly or Assemblies, but not both.");
                return false;
            }

            if (Assemblies != null && ConfigFile != null)
            {
                Log.LogError("If you specify the Assemblies property, you cannot specify the ConfigFile property.");
                return false;
            }

            return true;
        }
    }
}