using System.Xml;
using TestUtility;
using Xunit;

namespace Xunit1
{
    public class ExecutorAssemblyNodeCallbackAcceptanceTests : AcceptanceTestInNewAppDomain
    {
        [Fact]
        public void AssemblyFilenameInXmlMatchesOriginallyPassedNameToExecutor()
        {
            using (MockAssembly mockAssembly = new MockAssembly())
            {
                mockAssembly.Compile("");
                XmlNode assemblyNode = mockAssembly.Run(null);
                ResultXmlUtility.AssertAttribute(assemblyNode, "name", mockAssembly.FileName);
            }
        }
    }
}
