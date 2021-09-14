using System;
using System.Collections.Generic;
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
            if(!NoErrorColoring)
            {
                ConsoleHelper.SetForegroundColor(ConsoleColor.Yellow);
            }
            Console.WriteLine($"Skipping -{key} because XSL-T is not supported on .NET Core 1.x");
            if(!NoErrorColoring)
            {
                ConsoleHelper.ResetColor();
            }
#else
            var xmlTransform = new System.Xml.Xsl.XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true }))
            using (var xsltStream = typeof(TransformFactory).GetTypeInfo().Assembly.GetManifestResourceStream($"Xunit.ConsoleClient.{resourceName}"))
            using (var xsltReader = XmlReader.Create(xsltStream))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
#endif
        }
    }
}
