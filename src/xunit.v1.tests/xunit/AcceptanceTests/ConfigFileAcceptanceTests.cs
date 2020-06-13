using System.Xml;
using TestUtility;
using Xunit;

namespace Xunit1
{
    public class ConfigFileAcceptanceTests : AcceptanceTestInNewAppDomain
    {
        [Fact]
        public void LackOfConfigurationFileBugInCLR4()  // http://xunit.codeplex.com/workitem/9696
        {
            string code = @"
            using System;
            using Xunit;

            public class ConfigurationFileExample
            {
                [Fact]
                public void Test()
                {
                    new Uri(""http://localhost:58080/indexes/categoriesByName?query=CategoryName%3ABeverages&start=0&pageSize=25"");
                }
            }";

            XmlNode assemblyNode = Execute(code);

            ResultXmlUtility.AssertResult(assemblyNode, "Pass", "ConfigurationFileExample.Test");
        }
    }
}
