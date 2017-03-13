#if NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Xunit.ConsoleClient
{
    public class TransformFactory
    {
        static readonly TransformFactory instance = new TransformFactory();

        readonly Dictionary<string, Transform> availableTransforms = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        protected TransformFactory()
        {
            availableTransforms.Add("xml", new Transform { CommandLine = "xml", Description = "output results to xUnit.net v2 XML file", OutputHandler = Handler_DirectWrite });
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
            using (var stream = File.OpenWrite(outputFileName))
                xml.Save(stream);
        }
    }
}

#endif
