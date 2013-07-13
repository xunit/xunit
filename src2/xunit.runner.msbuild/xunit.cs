using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xunit.Runner.MSBuild
{
    public class xunit : Task, ICancelableTask
    {
        bool cancel;

        public xunit()
        {
            ShadowCopy = true;
            TeamCity = Environment.GetEnvironmentVariable("TEAMCITY_PROJECT_NAME") != null;
        }

        [Required]
        public ITaskItem[] Assemblies { get; set; }

        [Output]
        public int ExitCode { get; protected set; }

        public ITaskItem Html { get; set; }

        protected bool NeedsXml
        {
            get { return Xml != null || XmlV1 != null || Html != null; }
        }

        public bool ShadowCopy { get; set; }

        public bool TeamCity { get; set; }

        public bool Verbose { get; set; }

        public string WorkingFolder { get; set; }

        public ITaskItem Xml { get; set; }

        public ITaskItem XmlV1 { get; set; }

        public void Cancel()
        {
            cancel = true;
        }

        protected virtual IFrontController CreateFrontController(string assemblyFilename, string configFileName)
        {
            return new XunitFrontController(assemblyFilename, configFileName, ShadowCopy);
        }

        protected virtual MSBuildVisitor CreateVisitor(string assemblyFileName, XElement assemblyElement)
        {
            if (TeamCity)
                return new TeamCityVisitor(Log, assemblyElement, () => cancel, assemblyFileName);

            return new StandardOutputVisitor(Log, assemblyElement, Verbose, () => cancel);
        }

        public override bool Execute()
        {
            RemotingUtility.CleanUpRegisteredChannels();
            XElement assembliesElement = null;
            var environment = String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version);

            if (NeedsXml)
                assembliesElement = new XElement("assemblies");

            using (AssemblyHelper.SubscribeResolve())
            {
                if (WorkingFolder != null)
                    Directory.SetCurrentDirectory(WorkingFolder);

                Log.LogMessage(MessageImportance.High, "xUnit.net MSBuild runner ({0})", environment);

                foreach (ITaskItem assembly in Assemblies)
                {
                    if (cancel)
                        break;

                    string assemblyFileName = assembly.GetMetadata("FullPath");
                    string configFileName = assembly.GetMetadata("ConfigFile");
                    if (configFileName != null && configFileName.Length == 0)
                        configFileName = null;

                    var assemblyElement = CreateAssemblyXElement();
                    var visitor = CreateVisitor(assemblyFileName, assemblyElement);
                    ExecuteAssembly(assemblyFileName, configFileName, visitor);
                    visitor.Finished.WaitOne();

                    if (visitor.Failed != 0)
                        ExitCode = 1;

                    if (assembliesElement != null)
                        assembliesElement.Add(assemblyElement);
                }
            }

            if (NeedsXml)
            {
                if (Xml != null)
                    assembliesElement.Save(Xml.GetMetadata("FullPath"));

                if (XmlV1 != null)
                    Transform("xUnit1.xslt", assembliesElement, XmlV1);

                if (Html != null)
                    Transform("HTML.xslt", assembliesElement, Html);
            }

            return ExitCode == 0;
        }

        XElement CreateAssemblyXElement()
        {
            return NeedsXml ? new XElement("assembly") : null;
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

        void Transform(string resourceName, XNode xml, ITaskItem outputFile)
        {
            var xmlTransform = new XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFile.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
            using (var xsltReader = XmlReader.Create(typeof(xunit).Assembly.GetManifestResourceStream("Xunit.Runner.MSBuild." + resourceName)))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }
    }
}