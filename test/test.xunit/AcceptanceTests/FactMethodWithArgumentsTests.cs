using System;
using System.Xml;
using TestUtility;
using Xunit;

public class FactMethodWithArgumentsTests : AcceptanceTest
{
    [Fact]
    public void FactMethodsCannotHaveArguments()
    {
        string code = @"
            using System;
            using Xunit;

            public class MockTestClass
            {
                [Fact] public void FactWithParameters(int x) { }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Fail", "MockTestClass.FactWithParameters");
        XmlNode failureNode = testNode.SelectSingleNode("failure");
        ResultXmlUtility.AssertAttribute(failureNode, "exception-type", typeof(InvalidOperationException).FullName);
        Assert.Equal("System.InvalidOperationException : Fact method MockTestClass.FactWithParameters cannot have parameters", failureNode.SelectSingleNode("message").InnerText);
    }
}
