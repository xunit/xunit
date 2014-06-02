
#if !WIN8_STORE && !WINDOWS_PHONE_APP
using System.ComponentModel.Composition;
#endif
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Xunit.Runner.VisualStudio.Settings
{
#if !WIN8_STORE && !WINDOWS_PHONE_APP
    [Export(typeof(ISettingsProvider))]
#endif
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