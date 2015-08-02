using System;
using System.Collections.Generic;
using System.Configuration;
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
            var executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
            var exeConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var configSection = (XunitConsoleConfigurationSection)exeConfiguration.GetSection("xunit") ?? new XunitConsoleConfigurationSection();

            availableTransforms.Add("xml", new Transform { CommandLine = "xml", Description = "output results to xUnit.net v2 style XML file", OutputHandler = Handler_DirectWrite });

            configSection.Transforms.Cast<TransformConfigurationElement>().ToList().ForEach(configElement =>
            {
                string xslFileName = Path.Combine(executablePath, configElement.XslFile);
                if (!File.Exists(xslFileName))
                    throw new ArgumentException($"cannot find transform XSL file '{xslFileName}' for transform '{configElement.CommandLine}'");

                availableTransforms.Add(configElement.CommandLine,
                                        new Transform
                                        {
                                            CommandLine = configElement.CommandLine,
                                            Description = configElement.Description,
                                            OutputHandler = (xml, outputFileName) => Handler_XslTransform(xslFileName, xml, outputFileName)
                                        });
            });
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
            using (var xsltStream = File.Open(xslPath, FileMode.Open, FileAccess.Read))
            using (var xsltReader = XmlReader.Create(xsltStream))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }
    }
}
