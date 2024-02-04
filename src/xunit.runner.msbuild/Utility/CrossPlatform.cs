using System;
using System.Globalization;
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
            var fqResourceName = "Xunit.Runner.MSBuild." + resourceName;

            using (var writer = XmlWriter.Create(outputFile.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
            using (var xsltStream = typeof(xunit).Assembly.GetManifestResourceStream(fqResourceName))
            {
                if (xsltStream is null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not load resource '{0}' from assembly '{1}'", fqResourceName, typeof(xunit).Assembly.Location));

                using (var xsltReader = XmlReader.Create(xsltStream))
                using (var xmlReader = xml.CreateReader())
                {
                    xmlTransform.Load(xsltReader);
                    xmlTransform.Transform(xmlReader, writer);
                }
            }
        }

        public static Assembly LoadAssembly(string dllFile)
            => Assembly.LoadFile(dllFile);

        public static string Version => string.Format(CultureInfo.CurrentCulture, ".NET Framework {0}", Environment.Version);
    }
}
