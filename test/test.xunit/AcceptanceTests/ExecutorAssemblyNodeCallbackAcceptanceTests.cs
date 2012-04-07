using System.Xml;
using TestUtility;
using Xunit;

public class ExecutorAssemblyNodeCallbackAcceptanceTests : AcceptanceTest
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
