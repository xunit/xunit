using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    // REVIEW: Where is cancellation support?

    public class xunit : Task, ICancelableTask
    {
        bool cancel;

        public xunit()
        {
            ShadowCopy = true;
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
        }

        public ITaskItem[] Assemblies { get; set; }

        [Output]
        public int ExitCode { get; protected set; }

        //public ITaskItem Html { get; set; }

        public bool ShadowCopy { get; set; }

        public bool TeamCity { get; set; }

        public bool Verbose { get; set; }

        public string WorkingFolder { get; set; }

        //public ITaskItem Xml { get; set; }

        public void Cancel()
        {
            cancel = true;
        }

        protected virtual IFrontController CreateFrontController(string assemblyFilename, string configFileName)
        {
            return new XunitFrontController(assemblyFilename, configFileName, ShadowCopy);
        }

        protected virtual MSBuildVisitor CreateVisitor()
        {
            //TeamCity ? (IMessageSink)new TeamCityLogger(Log, () => cancel) :
            //Verbose ? new VerboseLogger(Log, () => cancel) :
            //          new StandardLogger(Log, () => cancel);
            return new StandardOutputVisitor(Log, () => cancel);
        }

        public override bool Execute()
        {
            RemotingUtility.CleanUpRegisteredChannels();

            using (AssemblyHelper.SubscribeResolve())
            {
                if (WorkingFolder != null)
                    Directory.SetCurrentDirectory(WorkingFolder);

                Log.LogMessage(MessageImportance.High, "xUnit.net MSBuild runner ({0}-bit .NET {1})", IntPtr.Size * 8, Environment.Version);

                MSBuildVisitor visitor = CreateVisitor();

                foreach (ITaskItem assembly in Assemblies)
                {
                    if (cancel)
                        break;

                    string assemblyFilename = assembly.GetMetadata("FullPath");
                    string configFilename = assembly.GetMetadata("ConfigFile");
                    if (configFilename.Length == 0)
                        configFilename = null;

                    ExecuteAssembly(assemblyFilename, configFilename, visitor);
                }

                return ExitCode == 0 && visitor.Failed == 0;
            }
        }

        protected virtual void ExecuteAssembly(string assemblyFilename, string configFileName, MSBuildVisitor resultsVisitor)
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Test assembly: {0}", assemblyFilename);

                using (var controller = CreateFrontController(assemblyFilename, configFileName))
                {
                    var discoveryVisitor = new TestDiscoveryVisitor();
                    controller.Find(includeSourceInformation: false, messageSink: discoveryVisitor);
                    discoveryVisitor.Finished.WaitOne();

                    controller.Run(discoveryVisitor.TestCases, resultsVisitor);
                    resultsVisitor.Finished.WaitOne();
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
        }

        //static Stream ResourceStream(string xmlResourceName)
        //{
        //    return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Xunit.Runner.MSBuild." + xmlResourceName);
        //}
    }
}