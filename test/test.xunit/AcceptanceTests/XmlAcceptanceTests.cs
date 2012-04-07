using System;
using System.Linq;
using System.Xml;
using TestUtility;
using Xunit;

public class XmlAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void EndToEndXmlValidation()
    {
        string code = @"
            using System;
            using Xunit;

            namespace Namespace1
            {
                public class Class1
                {
                    [Fact]
                    [Trait(""Bug"", ""123"")]
                    [Trait(""Bug"", ""456"")]
                    public void Passing()
                    {
                        Assert.Equal(2, 2);
                    }

                    [Fact]
                    public void Failing()
                    {
                        Assert.Equal(2, 3);
                    }

                    [Fact(Skip=""Skipping"")]
                    public void Skipped() {}
                }
            }

            namespace Namespace2
            {
                public class OuterClass
                {
                    public class Class2
                    {
                        [Fact]
                        public void Passing()
                        {
                            Assert.Equal(2, 2);
                        }
                    }
                }
            }
        ";

        XmlNode assemblyNode = Execute(code);

        Assert.Equal("4", assemblyNode.Attributes["total"].Value);
        Assert.Equal("2", assemblyNode.Attributes["passed"].Value);
        Assert.Equal("1", assemblyNode.Attributes["failed"].Value);
        Assert.Equal("1", assemblyNode.Attributes["skipped"].Value);

        XmlNodeList classNodes = assemblyNode.SelectNodes("class");
        Assert.Equal(classNodes.Count, 2);

        XmlNode class1Node = classNodes[0];
        Assert.Equal("Namespace1.Class1", class1Node.Attributes["name"].Value);

        XmlNodeList class1TestNodes = class1Node.SelectNodes("test");
        Assert.Equal(class1TestNodes.Count, 3);
        Assert.NotNull(class1Node.SelectSingleNode(@"//test[@name=""Namespace1.Class1.Passing""]"));
        Assert.NotNull(class1Node.SelectSingleNode(@"//test[@name=""Namespace1.Class1.Failing""]"));
        Assert.NotNull(class1Node.SelectSingleNode(@"//test[@name=""Namespace1.Class1.Skipped""]"));

        XmlNode passingNode = class1Node.SelectSingleNode(@"//test[@name=""Namespace1.Class1.Passing""]");
        XmlNodeList traitsNodes = passingNode.SelectNodes("traits/trait");
        Assert.Equal(2, traitsNodes.Count);
        Assert.True(traitsNodes.OfType<XmlNode>().Any(n => n.Attributes["name"].Value == "Bug" &&
                                                           n.Attributes["value"].Value == "123"));
        Assert.True(traitsNodes.OfType<XmlNode>().Any(n => n.Attributes["name"].Value == "Bug" &&
                                                           n.Attributes["value"].Value == "456"));

        XmlNode failingNode = class1Node.SelectSingleNode(@"//test[@name=""Namespace1.Class1.Failing""]");
        string stackTrace = failingNode.SelectSingleNode("failure/stack-trace").InnerText;
        Assert.Contains("at Namespace1.Class1.Failing", stackTrace);
        Assert.DoesNotContain("ExceptionUtility", stackTrace);

        XmlNode class2Node = classNodes[1];
        Assert.Equal("Namespace2.OuterClass+Class2", class2Node.Attributes["name"].Value);

        XmlNodeList class2TestNodes = class2Node.SelectNodes("test");
        Assert.Equal(class2TestNodes.Count, 1);
        Assert.Equal("Namespace2.OuterClass+Class2.Passing", class2TestNodes[0].Attributes["name"].Value);
        Assert.Equal("Namespace2.OuterClass+Class2", class2TestNodes[0].Attributes["type"].Value);
        Assert.Equal("Passing", class2TestNodes[0].Attributes["method"].Value);
    }

    [Fact]
    public void ExceptionCommentsCanIncludeCDATADeclarations()
    {
        string code =
        @"
                using System;
                using Xunit;

                public class Class1
                {
                    [Fact]
                    public void Failing()
                    {
                        throw new Exception(""<![CDATA[This is"" + Environment.NewLine + ""dummy CData]]>"");
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = assemblyNode.SelectSingleNode("class/test");
        Assert.Equal("System.Exception : <![CDATA[This is" + Environment.NewLine + "dummy CData]]>", testNode.SelectSingleNode("failure/message").InnerText);
    }
}