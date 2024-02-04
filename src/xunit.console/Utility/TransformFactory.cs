using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Xunit.ConsoleClient
{
    public class TransformFactory
    {
        static readonly TransformFactory instance = new TransformFactory();

        readonly Dictionary<string, Transform> availableTransforms = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        public static bool NoErrorColoring = false;

        protected TransformFactory()
        {
            availableTransforms.Add("xml", new Transform
            {
                CommandLine = "xml",
                Description = "output results to xUnit.net v2 XML file",
                OutputHandler = Handler_DirectWrite
            });
            availableTransforms.Add("xmlv1", new Transform
            {
                CommandLine = "xmlv1",
                Description = "output results to xUnit.net v1 XML file",
                OutputHandler = (xml, outputFileName) => Handler_XslTransform("xmlv1", "xUnit1.xslt", xml, outputFileName)
            });
            availableTransforms.Add("html", new Transform
            {
                CommandLine = "html",
                Description = "output results to HTML file",
                OutputHandler = (xml, outputFileName) => Handler_XslTransform("html", "HTML.xslt", xml, outputFileName)
            });
            availableTransforms.Add("nunit", new Transform
            {
                CommandLine = "nunit",
                Description = "output results to NUnit v2.5 XML file",
                OutputHandler = (xml, outputFileName) => Handler_XslTransform("nunit", "NUnitXml.xslt", xml, outputFileName)
            });
            availableTransforms.Add("junit", new Transform
            {
                CommandLine = "junit",
                Description = "output results to JUnit XML file",
                OutputHandler = (xml, outputFileName) => Handler_XslTransform("junit", "JUnitXml.xslt", xml, outputFileName)
            });
        }

        public static List<Transform> AvailableTransforms
            => instance.availableTransforms.Values.ToList();

        public static List<Action<XElement>> GetXmlTransformers(XunitProject project)
            => project.Output
                      .Select(output => new Action<XElement>(xml => instance.availableTransforms[output.Key].OutputHandler(xml, output.Value)))
                      .ToList();

        static void Handler_DirectWrite(XElement xml, string outputFileName)
        {
            using (var stream = File.Create(outputFileName))
                xml.Save(stream);
        }

        static void Handler_XslTransform(string key, string resourceName, XElement xml, string outputFileName)
        {
#if NETCOREAPP1_0
            if (!NoErrorColoring)
                ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);

            Console.WriteLine("Skipping -{0} because XSL-T is not supported on .NET Core 1.x", key);

            if (!NoErrorColoring)
                ConsoleHelper.ResetColor();
#else
            var xmlTransform = new System.Xml.Xsl.XslCompiledTransform();
            var fqResourceName = string.Format(CultureInfo.InvariantCulture, "Xunit.ConsoleClient.{0}", resourceName);

            using (var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true }))
            using (var xsltStream = typeof(TransformFactory).GetTypeInfo().Assembly.GetManifestResourceStream(fqResourceName))
            {
                if (xsltStream is null)
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not load resource '{0}' from assembly '{1}'", fqResourceName, typeof(TransformFactory).Assembly.Location));

                using (var xsltReader = XmlReader.Create(xsltStream))
                using (var xmlReader = xml.CreateReader())
                {
                    xmlTransform.Load(xsltReader);
                    xmlTransform.Transform(xmlReader, writer);
                }
            }
#endif
        }
    }
}
