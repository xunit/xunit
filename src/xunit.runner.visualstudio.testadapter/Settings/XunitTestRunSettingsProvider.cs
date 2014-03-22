using System.ComponentModel.Composition;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Xunit.Runner.VisualStudio.Settings
{
    [Export(typeof(ISettingsProvider))]
    [SettingsName(XunitTestRunSettings.SettingsName)]
    public class XunitTestRunSettingsProvider : ISettingsProvider
    {
        private readonly XmlSerializer serializer = new XmlSerializer(typeof(XunitTestRunSettings));

        public XunitTestRunSettings Settings { get; private set; }

        public void Load(XmlReader reader)
        {
            if (reader.Read() && reader.Name == XunitTestRunSettings.SettingsName)
                Settings = serializer.Deserialize(reader) as XunitTestRunSettings;
        }
    }
}