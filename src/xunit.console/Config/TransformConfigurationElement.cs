using System.Configuration;

namespace Xunit.ConsoleClient
{
    public class TransformConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("commandline", IsRequired = true, IsKey = true)]
        public string CommandLine
        {
            get { return (string)this["commandline"]; }
            set { this["commandline"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get { return (string)this["description"]; }
            set { this["description"] = value; }
        }

        [ConfigurationProperty("xslfile", IsRequired = true)]
        public string XslFile
        {
            get { return (string)this["xslfile"]; }
            set { this["xslfile"] = value; }
        }
    }
}