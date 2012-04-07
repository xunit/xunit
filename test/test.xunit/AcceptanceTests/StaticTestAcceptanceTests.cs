using System.Xml;
using TestUtility;
using Xunit;

public class StaticTestAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void ClassWithStaticTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public class StaticTests
                {
                    [Fact]
                    public static void StaticTestMethod()
                    {
                    }
                }";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "StaticTests.StaticTestMethod");
    }

    [Fact]
    public void StaticClassWithStaticTest()
    {
        string code =
            @"
                using System;
                using Xunit;

                public static class StaticTests
                {
                    [Fact]
                    public static void StaticTestMethod()
                    {
                    }
                }";

        XmlNode assemblyNode = Execute(code);

        ResultXmlUtility.AssertResult(assemblyNode, "Pass", "StaticTests.StaticTestMethod");
    }
}