using System.Xml;
using TestUtility;
using Xunit;

public class TestFixtureTests : AcceptanceTest
{
    [Fact]
    public void UsesSingleInstanceOfFixtureDataForAllTests()
    {
        string code =
            @"
                using System;
                using System.Diagnostics;
                using Xunit;

                public class TestFixtureTest : IUseFixture<object>
                {
                    public static object fixtureData = null;

                    public void SetFixture(object data)
                    {
                        if (fixtureData == null)
                            fixtureData = data;
                        else
                            Assert.Same(fixtureData, data);
                    }

                    [Fact]
                    public void Test1()
                    {
                    }

                    [Fact]
                    public void Test2()
                    {
                    }
                }";

        XmlNode assemblyNode = Execute(code);

        XmlNode result0 = ResultXmlUtility.GetResult(assemblyNode, 0);
        ResultXmlUtility.AssertAttribute(result0, "result", "Pass");
        XmlNode result1 = ResultXmlUtility.GetResult(assemblyNode, 1);
        ResultXmlUtility.AssertAttribute(result1, "result", "Pass");
    }
}