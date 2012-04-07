using System.Xml;
using TestUtility;
using Xunit;

public class TestNameAcceptanceTests : AcceptanceTest
{
    static string codeHeader = @"
        using System;
        using System.Collections.Generic;
        using System.Reflection;
        using Xunit;
        using Xunit.Sdk;

        public class SpecAttribute : FactAttribute
        {
            public SpecAttribute(string name)
            {
                DisplayName = name;
            }

            protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
            {
                yield return new FactCommand(method);
            }
        }
    ";

    [Fact]
    public void NamedPassingTest()
    {
        string code = codeHeader + @"
                public class ExampleSpecUsage
                {
                    [Spec(""Passing specification"")]
                    public void Passing()
                    {
                        Assert.True(true);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode resultNode = ResultXmlUtility.GetResult(assemblyNode);
        ResultXmlUtility.AssertAttribute(resultNode, "result", "Pass");
        ResultXmlUtility.AssertAttribute(resultNode, "name", "Passing specification");
    }

    [Fact]
    public void NamedFailingTest()
    {
        string code = codeHeader + @"
                public class ExampleSpecUsage
                {
                    [Spec(""Failing specification"")]
                    public void Failing()
                    {
                        Assert.True(false);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode resultNode = ResultXmlUtility.GetResult(assemblyNode);
        ResultXmlUtility.AssertAttribute(resultNode, "result", "Fail");
        ResultXmlUtility.AssertAttribute(resultNode, "name", "Failing specification");
    }

    [Fact]
    public void NamedSkippedTest()
    {
        string code = codeHeader + @"
                public class ExampleSpecUsage
                {
                    [Spec(""Skipped specification"", Skip=""Failing, not sure why..."")]
                    public void Skippy()
                    {
                        Assert.False(true);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode resultNode = ResultXmlUtility.GetResult(assemblyNode);
        ResultXmlUtility.AssertAttribute(resultNode, "result", "Skip");
        ResultXmlUtility.AssertAttribute(resultNode, "name", "Skipped specification");
    }
}