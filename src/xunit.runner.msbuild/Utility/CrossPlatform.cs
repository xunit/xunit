using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Xunit.Runner.MSBuild
{
    public class CrossPlatform
    {
        public static void Transform(IRunnerLogger logger, string outputDisplayName, string resourceName, XNode xml, ITaskItem outputFile)
        {
            var xmlTransform = new System.Xml.Xsl.XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFile.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
            using (var xsltReader = XmlReader.Create(typeof(xunit).Assembly.GetManifestResourceStream("Xunit.Runner.MSBuild." + resourceName)))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }

        public static Assembly LoadAssembly(string dllFile)
            => Assembly.LoadFile(dllFile);

        public static string Version => $"Desktop .NET {Environment.Version}";
    }
}
