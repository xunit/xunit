using System.Configuration;

namespace Xunit.ConsoleClient
{
    public class TransformConfigurationElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TransformConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TransformConfigurationElement)element).CommandLine;
        }
    }
}