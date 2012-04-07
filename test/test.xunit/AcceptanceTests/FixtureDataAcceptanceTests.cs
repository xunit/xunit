using System.Xml;
using TestUtility;
using Xunit;

public class FixtureDataAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void ClassWithFixtureAndSkippedFactDoesNotSetFixtureData()
    {
        string code = @"
                using Xunit;

                public class MyFacts : IUseFixture<object>
                {
                    public void SetFixture(object data)
                    {
                        Assert.True(false);
                    }

                    [Fact(Skip=""Skip Me!"")]
                    public void SkippedTest() {}
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode result = ResultXmlUtility.GetResult(assemblyNode);
        ResultXmlUtility.AssertAttribute(result, "result", "Skip");
    }

    [Fact]
    public void ClassWithFixtureAndStaticFactDoesNotSetFixtureData()
    {
        string code = @"
                using Xunit;

                public class MyFacts : IUseFixture<object>
                {
                    public void SetFixture(object data)
                    {
                        Assert.True(false);
                    }

                    [Fact]
                    public static void StaticPassingTest() {}
                }
            ";

        XmlNode assemblyNode = Execute(code);

        XmlNode result = ResultXmlUtility.GetResult(assemblyNode);
        ResultXmlUtility.AssertAttribute(result, "result", "Pass");
    }
}