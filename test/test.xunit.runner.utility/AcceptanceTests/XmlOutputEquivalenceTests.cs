using System.Xml;
using TestUtility;
using Xunit;

public class XmlOutputEquivalenceTests : AcceptanceTestInNewAppDomain
{
    [Fact]
    public void XmlFromExecutorAndMateIsEquivalent()
    {
        string code = @"
            using Xunit;

            public class TestClass
            {
                [Fact]
                public void TestMethod() {}
            }
        ";

        XmlNode xmlFromExecutorWrapper;
        XmlNode xmlFromMate;

        using (MockAssembly mockAssembly = new MockAssembly())
        {
            mockAssembly.Compile(code);
            xmlFromExecutorWrapper = mockAssembly.Run();
            xmlFromMate = mockAssembly.RunWithMate();
        }

        // Make sure that we have these (and only these) attributes
        AssertExactAttributeList(
            xmlFromExecutorWrapper,
            "name", "configFile", "total", "passed", "failed", "skipped", "environment",
            "time", "run-date", "run-time", "test-framework"
        );
        AssertExactAttributeList(
            xmlFromMate,
            "name", "configFile", "total", "passed", "failed", "skipped", "environment",
            "time", "run-date", "run-time", "test-framework"
        );

        // Only compare values on assembly node, because we know that MATE
        // uses the actual XML from Executor for classes & tests. We can't
        // do equivalence for "time", "run-date", "run-time" because of variance.
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "name");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "configFile");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "total");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "passed");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "failed");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "skipped");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "environment");
        AssertAttributeEqual(xmlFromExecutorWrapper, xmlFromMate, "test-framework");
    }

    void AssertAttributeEqual(XmlNode node1, XmlNode node2, string attributeName)
    {
        Assert.Equal(
            node1.Attributes[attributeName].Value,
            node2.Attributes[attributeName].Value
        );
    }

    void AssertExactAttributeList(XmlNode node, params string[] attributeNames)
    {
        Assert.Equal(node.Attributes.Count, attributeNames.Length);

        foreach (string attributeName in attributeNames)
        {
            var attribute = node.Attributes[attributeName];
            Assert.NotNull(attribute);
            Assert.NotEmpty(attribute.Value);
        }
    }
}
