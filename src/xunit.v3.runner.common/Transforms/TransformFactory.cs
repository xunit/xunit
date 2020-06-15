using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// Used to retrieve a list of available
    /// </summary>
    public class TransformFactory
    {
        static readonly TransformFactory instance = new TransformFactory();

        readonly List<Transform> availableTransforms;

        TransformFactory()
        {
            availableTransforms = new List<Transform> {
                new Transform
                {
                    ID = "xml",
                    Description = "output results to xUnit.net v2+ XML file",
                    OutputHandler = Handler_DirectWrite
                },
                new Transform
                {
                    ID = "xmlv1",
                    Description = "output results to xUnit.net v1 XML file",
                    OutputHandler = (xml, outputFileName) => Handler_XslTransform("xUnit1.xslt", xml, outputFileName)
                },
                new Transform
                {
                    ID = "html",
                    Description = "output results to HTML file",
                    OutputHandler = (xml, outputFileName) => Handler_XslTransform("HTML.xslt", xml, outputFileName)
                },
                new Transform
                {
                    ID = "nunit",
                    Description = "output results to NUnit v2.5 XML file",
                    OutputHandler = (xml, outputFileName) => Handler_XslTransform("NUnitXml.xslt", xml, outputFileName)
                },
                new Transform
                {
                    ID = "junit",
                    Description = "output results to JUnit XML file",
                    OutputHandler = (xml, outputFileName) => Handler_XslTransform("JUnitXml.xslt", xml, outputFileName)
                }
            };
        }

        /// <summary>
        /// Gets the list of available transforms.
        /// </summary>
        public static IReadOnlyList<Transform> AvailableTransforms =>
            instance.availableTransforms;

        /// <summary>
        /// Gets the list of XML transformer functions for the given project.
        /// </summary>
        /// <param name="project">The project to get transforms for.</param>
        /// <returns>The list of transform functions.</returns>
        public static List<Action<XElement>> GetXmlTransformers(XunitProject project) =>
            project.Output
                .Select(output => new Action<XElement>(xml => instance.availableTransforms.Single(t => t.ID == output.Key).OutputHandler(xml, output.Value)))
                .ToList();

        static void Handler_DirectWrite(XElement xml, string outputFileName)
        {
            using var stream = File.Create(outputFileName);
            xml.Save(stream);
        }

        static void Handler_XslTransform(string resourceName, XElement xml, string outputFileName)
        {
            var xmlTransform = new XslCompiledTransform();

            using var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true });
            using var xsltStream = typeof(TransformFactory).GetTypeInfo().Assembly.GetManifestResourceStream($"Xunit.Runner.Common.Transforms.templates.{resourceName}");
            using var xsltReader = XmlReader.Create(xsltStream);
            using var xmlReader = xml.CreateReader();
            xmlTransform.Load(xsltReader);
            xmlTransform.Transform(xmlReader, writer);
        }
    }
}
