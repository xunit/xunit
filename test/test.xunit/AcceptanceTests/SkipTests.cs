using System.Xml;
using TestUtility;
using Xunit;

public class SkipTests : AcceptanceTest
{
    [Fact]
    public void TestIsMarkedSkippedAndDoesNotExecute()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    [Fact(Skip = ""the reason"")]
                    public void FailedTestThatShouldBeSkipped()
                    {
                        Assert.Equal(2, 3);
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Skip", "MockTestClass.FailedTestThatShouldBeSkipped");
        XmlNode messageNode = testNode.SelectSingleNode("reason/message");
        Assert.Equal("the reason", messageNode.InnerText);
    }

    [Fact]
    public void TestClassIsNotInstantiatedForSkippedTests()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class MockTestClass
                {
                    public MockTestClass()
                    {
                        throw new Exception(""Should not reach me!"");
                    }

                    [Fact(Skip = ""the reason"")]
                    public void TestThatShouldBeSkipped()
                    {
                    }
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode testNode = ResultXmlUtility.AssertResult(assemblyNode, "Skip", "MockTestClass.TestThatShouldBeSkipped");
        XmlNode messageNode = testNode.SelectSingleNode("reason/message");
        Assert.Equal("the reason", messageNode.InnerText);
    }
}