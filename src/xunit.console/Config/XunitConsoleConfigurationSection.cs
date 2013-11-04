using System.Configuration;

namespace Xunit.ConsoleClient
{
    public class XunitConsoleConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("transforms", IsDefaultCollection = false)]
        public TransformConfigurationElementCollection Transforms
        {
            get { return (TransformConfigurationElementCollection)this["transforms"]; }
            set { this["transforms"] = value; }
        }
    }
}