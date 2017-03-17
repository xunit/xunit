#if NET452

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Xunit.ConsoleClient
{
    public class TransformFactory
    {
        static readonly TransformFactory instance = new TransformFactory();

        readonly Dictionary<string, Transform> availableTransforms = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        protected TransformFactory()
        {
            availableTransforms.Add("xml", new Transform { CommandLine = "xml", Description = "output results to xUnit.net v2 XML file", OutputHandler = Handler_DirectWrite });

            var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
            var configFilePath = Path.Combine(executablePath, "xunit.console.json");
            if (!File.Exists(configFilePath))
                return;

            using (var configStream = File.OpenRead(configFilePath))
            using (var reader = new StreamReader(configStream))
            {
                var config = JsonDeserializer.Deserialize(reader) as JsonObject;
                var xslTransforms = config.ValueAsJsonObject("xslTransforms");
                if (xslTransforms != null)
                    foreach (var commandLine in xslTransforms.Keys)
                    {
                        var transform = xslTransforms.ValueAsJsonObject(commandLine);
                        var xslFilePath = Path.Combine(executablePath, transform.ValueAsString("file"));
                        if (!File.Exists(xslFilePath))
                            throw new ArgumentException($"cannot find transform XSL file '{xslFilePath}' for transform '{commandLine}'");

                        availableTransforms.Add(commandLine,
                                                new Transform
                                                {
                                                    CommandLine = commandLine,
                                                    Description = transform.ValueAsString("description"),
                                                    OutputHandler = (xml, outputFileName) => Handler_XslTransform(xslFilePath, xml, outputFileName)
                                                });
                    }

            }
        }

        public static List<Transform> AvailableTransforms
        {
            get { return instance.availableTransforms.Values.ToList(); }
        }

        public static List<Action<XElement>> GetXmlTransformers(XunitProject project)
        {
            return project.Output.Select(output => new Action<XElement>(xml => instance.availableTransforms[output.Key].OutputHandler(xml, output.Value))).ToList();
        }

        static void Handler_DirectWrite(XElement xml, string outputFileName)
        {
            xml.Save(outputFileName);
        }

        static void Handler_XslTransform(string xslPath, XElement xml, string outputFileName)
        {
            var xmlTransform = new XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true }))
            using (var xsltStream = File.Open(xslPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var xsltReader = XmlReader.Create(xsltStream))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }
    }
}

#endif
