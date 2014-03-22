using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Xunit.Runner.VisualStudio.Settings
{
    public class XunitTestRunSettings : TestRunSettings
    {
        public const string SettingsName = "XunitTestRunSettings";

        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(XunitTestRunSettings));

        public XunitTestRunSettings() : base(SettingsName) { }

        public int? MaxParallelThreads { get; set; }
        public MessageDisplay? MessageDisplay { get; set; }
        public NameDisplay? NameDisplay { get; set; }
        public bool? ParallelizeAssemblies { get; set; }
        public bool? ParallelizeTestCollections { get; set; }
        public bool? ShutdownAfterRun { get; set; }

        public override XmlElement ToXml()
        {
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, this);
            var xml = stringWriter.ToString();
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document.DocumentElement;
        }
    }
}
